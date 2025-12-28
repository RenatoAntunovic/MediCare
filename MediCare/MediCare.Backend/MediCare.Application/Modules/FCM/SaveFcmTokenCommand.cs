using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.FCM
{
    public class SaveFcmTokenCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string Token { get; set; }

        public SaveFcmTokenCommand(int userId,string token)
        {
            UserId = userId;
            Token = token;
        }
    }
}
