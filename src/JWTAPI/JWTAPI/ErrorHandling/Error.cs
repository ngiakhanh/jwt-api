namespace JWTAPI.ErrorHandling
{
    public class Error
    {
        /// <summary>
        /// Message of the error.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Error type. Example: 'ArgumentException'
        /// </summary>
        public string Type { get; set; }
    }
}