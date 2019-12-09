namespace WebSakFilopplaster.Net_AD
{
    /**
     * Hjelpeklasse for å returnere detaljert informasjon fra metoder,
     * for å vise en mer informativ feilmelding på nettsiden.
     */
    public class Response<T>
    {
        public T Value { private get; set; }
        public string Message { get; set; }
        public Codes.Code Code { get; set; }
        public bool Success { get { return Code == Codes.Code.OK; } }
        public T Get()
        {
            return Value;
        }


        public Response(T value, string message, Codes.Code code)
        {
            Value = value;
            Message = message;
            Code = code;
        }
    }
}