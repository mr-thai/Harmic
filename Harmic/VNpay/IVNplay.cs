using VNPAY.NET;

public class VnpayPayment
{
    private string _tmnCode;
    private string _hashSecret;
    private string _baseUrl;
    private string _callbackUrl;

    private readonly IVnpay _vnpay;

    public VnpayPayment()
    {
        _vnpay = new Vnpay();
        _vnpay.Initialize(_tmnCode, _hashSecret, _baseUrl, _callbackUrl);
    }
}