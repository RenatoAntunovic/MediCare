using MediCare.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.FCM
{
    public class SaveFcmTokenHandler : IRequestHandler<SaveFcmTokenCommand, bool>
    {
        private readonly IAppDbContext _context;

        public SaveFcmTokenHandler(IAppDbContext context)
        {
            _context = context;
        }

        // Thread-safe memorija za testiranje (ako baza nije potrebna)
        private static readonly ConcurrentDictionary<int, string> _tokens = new();

        public async Task<bool> Handle(SaveFcmTokenCommand request, CancellationToken cancellationToken)
        {
            // 1️⃣ Spremi u memoriju (za test)
            _tokens[request.UserId] = request.Token;

            // 2️⃣ Spremi u bazu
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null)
                return false;

            user.FcmToken = request.Token;
            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"FCM token saved: UserId={request.UserId}, Token={request.Token}");

            return true;
        }

        // Opcionalno: metoda za dohvat tokena (za slanje notifikacija)
        public static bool TryGetToken(int userId, out string token)
        {
            return _tokens.TryGetValue(userId, out token);
        }

        public static IReadOnlyDictionary<int, string> GetAllTokens()
        {
            return _tokens;
        }
    }
}
