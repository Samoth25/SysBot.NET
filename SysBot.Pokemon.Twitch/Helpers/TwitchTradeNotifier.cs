using PKHeX.Core;
using SysBot.Base;
using System;
using System.Linq;
using TwitchLib.Client;

namespace SysBot.Pokemon.Twitch
{
    public class TwitchTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        private T Data { get; }
        private PokeTradeTrainerInfo Info { get; }
        private int Code { get; }
        private string Username { get; }
        private TwitchClient Client { get; }
        private string Channel { get; }
        private TwitchSettings Settings { get; }

        public TwitchTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string username, TwitchClient client, string channel, TwitchSettings settings)
        {
            Data = data;
            Info = info;
            Code = code;
            Username = username;
            Client = client;
            Channel = channel;
            Settings = settings;

            LogUtil.LogText($"Détails de l'échange crées pour {Username} - {Code}");
        }

        public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        {
            LogUtil.LogText(message);
            SendMessage($"@{info.Trainer.TrainerName}: {message}", Settings.NotifyDestination);
        }

        public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            OnFinish?.Invoke(routine);
            var line = $"@{info.Trainer.TrainerName}: Commande annulée, {msg}";
            LogUtil.LogText(line);
            SendMessage(line, Settings.TradeCanceledDestination);
        }

        public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
        {
            OnFinish?.Invoke(routine);
            var tradedToUser = Data.Species;
            var message = $"@{info.Trainer.TrainerName}: " + (tradedToUser != 0 ? $"Echange terminé. Profite de ton {(Species)tradedToUser}!" : "Echange terminé !");
            LogUtil.LogText(message);
            SendMessage(message, Settings.TradeFinishDestination);
        }

        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
            var msg = $"@{info.Trainer.TrainerName} (ID: {info.ID}): Je commence l'échange de{receive} avec toi. Merci d'être prêt. Utilise le code que tu m'as envoyé par MP pour l'échange.";
            var dest = Settings.TradeStartDestination;
            if (dest == TwitchMessageDestination.Whisper)
                msg += $" Ton code d'échange est : {info.Code:0000 0000}";
            LogUtil.LogText(msg);
            SendMessage(msg, dest);
        }

        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", @{name}";
            var message = $"Je t'attends{trainer}! Mon nom dans le jeu est {routine.InGameName}.";
            var dest = Settings.TradeSearchDestination;
            if (dest == TwitchMessageDestination.Channel)
                message += " Utilise le code que tu m'as envoyé par MP pour l'échange !";
            else if (dest == TwitchMessageDestination.Whisper)
                message += $" Ton code d'échange est : {info.Code:0000 0000}";
            LogUtil.LogText(message);
            SendMessage($"@{info.Trainer.TrainerName} {message}", dest);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
        {
            var msg = message.Summary;
            if (message.Details.Count > 0)
                msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
            LogUtil.LogText(msg);
            SendMessage(msg, Settings.NotifyDestination);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
        {
            var msg = $"Détails pour {result.FileName}: " + message;
            LogUtil.LogText(msg);
            SendMessage(msg, Settings.NotifyDestination);
        }

        private void SendMessage(string message, TwitchMessageDestination dest)
        {
            switch (dest)
            {
                case TwitchMessageDestination.Channel:
                    Client.SendMessage(Channel, message);
                    break;
                case TwitchMessageDestination.Whisper:
                    Client.SendWhisper(Username, message);
                    break;
            }
        }
    }
}
