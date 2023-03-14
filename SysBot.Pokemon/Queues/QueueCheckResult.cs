using PKHeX.Core;

namespace SysBot.Pokemon
{
    /// <summary>
    /// Stores data for indicating how a queue position/presence check resulted.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed record QueueCheckResult<T> where T : PKM, new()
    {
        public readonly bool InQueue;
        public readonly TradeEntry<T>? Detail;
        public readonly int Position;
        public readonly int QueueCount;

        public static readonly QueueCheckResult<T> None = new();

        public QueueCheckResult(bool inQueue = false, TradeEntry<T>? detail = default, int position = -1, int queueCount = -1)
        {
            InQueue = inQueue;
            Detail = detail;
            Position = position;
            QueueCount = queueCount;
        }

        public string GetMessage()
        {
            if (!InQueue || Detail is null)
                return "Tu n'es pas dans la file d'attente actuellement.";
            var position = $"{Position}/{QueueCount}";
            var msg = $"Tu es dans la file d'attente pour : {Detail.Type} Position : {position} (ID {Detail.Trade.ID})";
            var pk = Detail.Trade.TradeData;
            if (pk.Species != 0)
                msg += $", Pokémon demandé : {(Species)Detail.Trade.TradeData.Species}";
            return msg;
        }
    }
}
