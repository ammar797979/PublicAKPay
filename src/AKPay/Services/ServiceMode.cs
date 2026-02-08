namespace AKPay.Services
{
    public enum ServiceFlavor
    {
        Linq,
        Sproc
    }

    public static class ServiceMode
    {
        public static ServiceFlavor Current { get; set; } = ServiceFlavor.Linq;
    }
}
