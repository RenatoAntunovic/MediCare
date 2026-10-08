using System.Globalization;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using MediCare.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediCare.Infrastructure.Services;

/// <summary>
/// Full-text medicine search powered by Lucene.Net, running inside the API process
/// (no external server, no SQL Server Full-Text feature required).
///
/// - The index lives in memory (RAMDirectory) and is rebuilt from the database on every startup.
/// - It is kept up to date by the medicine handlers (create / update / delete / enable / disable).
/// - Matching: every word of the query must match (AND), each word as a prefix of the name,
///   category or description, plus typo tolerance (fuzzy) on the name.
/// - Diacritics are folded, so "glavobolja", "cetiri" and "četiri" all behave as expected
///   (č, ć, š, ž, đ are searchable with or without the diacritic).
/// </summary>
public sealed class LuceneMedicineSearchService : IMedicineSearchService, IMedicineSearchIndex, IDisposable
{
    private const LuceneVersion Version = LuceneVersion.LUCENE_48;

    private const string IdField = "id";
    private const string NameField = "name";
    private const string DescriptionField = "description";
    private const string CategoryField = "category";
    private const string PriceField = "price";
    private const string ImageField = "image";
    private const string WeightField = "weight";

    /// <summary>Maximum number of distinct words taken from one query.</summary>
    private const int MaxQueryTerms = 5;

    private readonly ILogger<LuceneMedicineSearchService> _logger;
    private readonly Analyzer _analyzer;
    private readonly RAMDirectory _directory;
    private readonly IndexWriter _writer;
    private readonly object _writeLock = new();

    public LuceneMedicineSearchService(ILogger<LuceneMedicineSearchService> logger)
    {
        _logger = logger;
        _analyzer = new FoldingAnalyzer();
        _directory = new RAMDirectory();
        _writer = new IndexWriter(_directory, new IndexWriterConfig(Version, _analyzer));
    }

    // =========================================================
    // SEARCH
    // =========================================================

    public Task<MedicineSearchResultDto> SearchAsync(string query, int page, int pageSize, CancellationToken ct)
    {
        var tokens = Tokenize(query);
        if (tokens.Count == 0)
            return Task.FromResult(new MedicineSearchResultDto());

        var luceneQuery = BuildQuery(tokens);

        // Near-real-time reader: sees everything written so far, even without a commit.
        using var reader = DirectoryReader.Open(_writer, true);
        var searcher = new IndexSearcher(reader);

        var top = searcher.Search(luceneQuery, page * pageSize);

        var items = top.ScoreDocs
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(hit => ToDto(searcher.Doc(hit.Doc)))
            .ToList();

        return Task.FromResult(new MedicineSearchResultDto
        {
            Total = top.TotalHits,
            Items = items
        });
    }

    /// <summary>
    /// (word1 in name|category|description) AND (word2 in ...) AND ...
    /// Name matches rank highest, then category, then description.
    /// </summary>
    private static Query BuildQuery(IReadOnlyList<string> tokens)
    {
        var all = new BooleanQuery();

        foreach (var token in tokens)
        {
            var perToken = new BooleanQuery(); // OR across fields for this word

            perToken.Add(Boosted(new PrefixQuery(new Term(NameField, token)), 3f), Occur.SHOULD);
            perToken.Add(Boosted(new PrefixQuery(new Term(CategoryField, token)), 1.5f), Occur.SHOULD);
            perToken.Add(new PrefixQuery(new Term(DescriptionField, token)), Occur.SHOULD);

            // Typo tolerance on the name only; the first letter must match to limit false positives.
            var maxEdits = token.Length >= 6 ? 2 : token.Length >= 4 ? 1 : 0;
            if (maxEdits > 0)
                perToken.Add(Boosted(new FuzzyQuery(new Term(NameField, token), maxEdits, 1), 2f), Occur.SHOULD);

            all.Add(perToken, Occur.MUST);
        }

        return all;
    }

    private static Query Boosted(Query query, float boost)
    {
        query.Boost = boost;
        return query;
    }

    /// <summary>Runs the user input through the same analyzer that was used for indexing.</summary>
    private List<string> Tokenize(string text)
    {
        var tokens = new List<string>();

        using var stream = _analyzer.GetTokenStream(NameField, text);
        var term = stream.AddAttribute<ICharTermAttribute>();

        stream.Reset();
        while (stream.IncrementToken())
            tokens.Add(term.ToString());
        stream.End();

        return tokens.Distinct().Take(MaxQueryTerms).ToList();
    }

    // =========================================================
    // INDEX MAINTENANCE
    // =========================================================

    public Task UpsertAsync(MedicineSearchItemDto item, CancellationToken ct)
    {
        try
        {
            _writer.UpdateDocument(
                new Term(IdField, item.Id.ToString(CultureInfo.InvariantCulture)),
                ToDocument(item));
        }
        catch (Exception ex)
        {
            // The index must never break saving to the database.
            _logger.LogWarning(ex, "Lucene upsert for medicine {Id} failed.", item.Id);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(int medicineId, CancellationToken ct)
    {
        try
        {
            _writer.DeleteDocuments(new Term(IdField, medicineId.ToString(CultureInfo.InvariantCulture)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lucene delete for medicine {Id} failed.", medicineId);
        }

        return Task.CompletedTask;
    }

    public Task RebuildAsync(IReadOnlyList<MedicineSearchItemDto> items, CancellationToken ct)
    {
        lock (_writeLock)
        {
            _writer.DeleteAll();

            foreach (var item in items)
                _writer.AddDocument(ToDocument(item));
        }

        return Task.CompletedTask;
    }

    // =========================================================
    // MAPPING
    // =========================================================

    private static Document ToDocument(MedicineSearchItemDto item)
    {
        var doc = new Document();

        // Not analyzed: exact key used for update / delete.
        doc.Add(new StringField(IdField, item.Id.ToString(CultureInfo.InvariantCulture), Field.Store.YES));

        // Analyzed + stored: searched and returned in results.
        doc.Add(new TextField(NameField, item.Name ?? string.Empty, Field.Store.YES));
        doc.Add(new TextField(DescriptionField, item.Description ?? string.Empty, Field.Store.YES));
        doc.Add(new TextField(CategoryField, item.Category ?? string.Empty, Field.Store.YES));

        // Stored only (returned in results, never searched). Strings avoid any decimal/double precision issues.
        doc.Add(new StoredField(PriceField, item.Price.ToString(CultureInfo.InvariantCulture)));
        doc.Add(new StoredField(ImageField, item.ImagePath ?? string.Empty));
        doc.Add(new StoredField(WeightField, item.Weight.ToString(CultureInfo.InvariantCulture)));

        return doc;
    }

    private static MedicineSearchItemDto ToDto(Document doc)
    {
        decimal.TryParse(doc.Get(PriceField), NumberStyles.Number, CultureInfo.InvariantCulture, out var price);
        int.TryParse(doc.Get(WeightField), NumberStyles.Integer, CultureInfo.InvariantCulture, out var weight);
        int.TryParse(doc.Get(IdField), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id);

        return new MedicineSearchItemDto
        {
            Id = id,
            Name = doc.Get(NameField) ?? string.Empty,
            Description = doc.Get(DescriptionField) ?? string.Empty,
            Category = doc.Get(CategoryField) ?? string.Empty,
            Price = price,
            ImagePath = doc.Get(ImageField) ?? string.Empty,
            Weight = weight
        };
    }

    public void Dispose()
    {
        _writer.Dispose();
        _directory.Dispose();
        _analyzer.Dispose();
    }

    // =========================================================
    // ANALYZER
    // =========================================================

    /// <summary>
    /// Standard tokenizer → lower-case → fold diacritics (č→c, ć→c, š→s, ž→z, đ→d).
    /// The same analyzer is used for indexing and for the user query, so both sides always match.
    /// </summary>
    private sealed class FoldingAnalyzer : Analyzer
    {
        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Tokenizer source = new StandardTokenizer(Version, reader);
            TokenStream result = new StandardFilter(Version, source);
            result = new LowerCaseFilter(Version, result);
            result = new ASCIIFoldingFilter(result);

            return new TokenStreamComponents(source, result);
        }
    }
}
