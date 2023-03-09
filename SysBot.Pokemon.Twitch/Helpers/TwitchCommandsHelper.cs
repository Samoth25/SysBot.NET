using System;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon.Twitch
{
    public static class TwitchCommandsHelper<T> where T : PKM, new()
    {
        // Helper functions for commands
        public static bool AddToWaitingList(string setstring, string display, string username, ulong mUserId, bool sub, out string msg)
        {
            if (!TwitchBot<T>.Info.GetCanQueue())
            {
                msg = "Désolé, je n'accepte pas de nouvelles commandes pour le moment.";
                return false;
            }

            var set = ShowdownUtil.ConvertToShowdown(setstring);
            if (set == null)
            {
                msg = $"Annulation de ta commande, @{username}: tu n'as pas fourni de surnom pour le Pokémon.";
                return false;
            }
            var template = AutoLegalityWrapper.GetTemplate(set);
            if (template.Species < 1)
            {
                msg = $"Annulation de ta commande, @{username}: Ce Pokémon n'existe pas ou n'est pas écrit en Anglais.";
                return false;
            }

            if (set.InvalidLines.Count != 0)
            {
                msg = $"Annulation de ta commande, @{username}: Le set Showdown n'est pas légal :\n{string.Join("\n", set.InvalidLines)}";
                return false;
            }

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                PKM pkm = sav.GetLegal(template, out var result);

                if (!pkm.CanBeTraded())
                {
                    msg = $"Annulation de ta commande, @{username}: Le Pokémon demandé n'est pas valide pour un échange.";
                    return false;
                }

                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid)
                    {
                        var tq = new TwitchQueue<T>(pk, new PokeTradeTrainerInfo(display, mUserId), username, sub);
                        TwitchBot<T>.QueuePool.RemoveAll(z => z.UserName == username); // remove old requests if any
                        TwitchBot<T>.QueuePool.Add(tq);
                        msg = $"@{username} - ta commande est enregistrée ! Merci de m'envoyer le code d'échange de ton choix à 8 chiffres en chuchotement (MP) ! Fais vite ou ta commande sera annulée !";
                        return true;
                    }
                }

                var reason = result == "Timeout" ? "Set took too long to generate." : "Unable to legalize the Pokémon.";
                msg = $"Skipping trade, @{username}: {reason}";
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(TwitchCommandsHelper<T>));
                msg = $"Annulation de ta commande, @{username}: Un problème avec le bot est survenu.";
            }
            return false;
        }

        public static string ClearTrade(string user)
        {
            var result = TwitchBot<T>.Info.ClearTrade(user);
            return GetClearTradeMessage(result);
        }

        public static string ClearTrade(ulong userID)
        {
            var result = TwitchBot<T>.Info.ClearTrade(userID);
            return GetClearTradeMessage(result);
        }

        private static string GetClearTradeMessage(QueueResultRemove result)
        {
            return result switch
            {
                QueueResultRemove.CurrentlyProcessing => "On dirait que ta commande est en cours d'échange. Impossible de t'enlever de la file d'attente.",
                QueueResultRemove.CurrentlyProcessingRemoved => "Looks like you're currently being processed! Removed from queue.",
                QueueResultRemove.Removed => "Tu as bien été enlevé de la file d'attente",
                _ => "Désolé mais tu n'es dans aucune file d'attente pour le moment.",
            };
        }

        public static string GetCode(ulong parse)
        {
            var detail = TwitchBot<T>.Info.GetDetail(parse);
            return detail == null
                ? "Désolé mais tu n'es dans aucune file d'attente pour le moment."
                : $"Ton code d'échange est : {detail.Trade.Code:0000 0000}";
        }
    }
}
