namespace TupiJua.Helpers
{
    /// <summary>
    /// Fornece métodos auxiliares para obter data/hora no fuso horário de São Paulo (GMT-3).
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// Fuso horário de São Paulo (E. South America Standard Time / America/Sao_Paulo).
        /// </summary>
        public static readonly TimeZoneInfo SaoPauloTimeZone = InitTimeZone();

        /// <summary>
        /// Obtém a data e hora atual no fuso horário de São Paulo.
        /// </summary>
        public static DateTimeOffset Now =>
            TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, SaoPauloTimeZone);

        /// <summary>
        /// Obtém a data atual (meia-noite) no fuso horário de São Paulo como <see cref="DateTimeOffset"/>.
        /// </summary>
        public static DateTimeOffset Today
        {
            get
            {
                var now = Now;
                return new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
            }
        }

        private static TimeZoneInfo InitTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
        }
    }
}
