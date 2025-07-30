using Microsoft.AspNetCore.SignalR;

namespace Reconova.Hubs
{
    public class CallHub : Hub
    {
        public async Task SendOffer(string to, string offer)
            => await Clients.User(to).SendAsync("ReceiveOffer", Context.UserIdentifier, offer);
        public async Task SendAnswer(string to, string answer)
            => await Clients.User(to).SendAsync("ReceiveAnswer", Context.UserIdentifier, answer);

        public async Task SendCandidate(string to, string candidate)
            => await Clients.User(to).SendAsync("ReceiveCandidate", Context.UserIdentifier, candidate);
    }
}
