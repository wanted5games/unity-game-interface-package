using System;
using System.Threading.Tasks;

public partial class GameInterface
{
    public event Action<IAPEvent> OnIAPEvent;
    /// <summary>
    /// Call GameInterface.Instance.GetIAPProducts() ONCE at the start of your game (after the GameInterface.init method has been resolved) to retrieve the list of available items. This method returns a Promise resolving to an array of products.
    /// <para>Empty Array: If the array is empty, no IAP items are available. In this case:</para>
    /// <list type="bullet">
    /// <item>Do not display IAP options in the shop.</item>
    /// <item>If no other ways to obtain products exist, consider hiding the shop entirely.</item>
    /// </list>
    /// <para>During testing, the order of the items may change, and not all items may be available intentionally. This is done to ensure that the implementation handles the product list correctly.
    /// In addition, the currency may also change, which will affect the previously specified price in USD. Therefore, the current displayPrice must always be used.</para>
    /// </summary>
    /// <returns></returns>
    public Task<IAPProduct[]> GetIAPProducts(Action<IAPProduct[]> onComplete = null)
    {
        return ExecuteWebGLRequest<IAPProduct[]>(id => GameInterfaceBridge.GetIAPProducts(id), onComplete);
    }


    public Task<PurchaseResult> BuyProduct(string sku, Action<PurchaseResult> onComplete = null)
    {
        return ExecuteWebGLRequest<PurchaseResult>(id => GameInterfaceBridge.BuyIAPProduct(id, sku), onComplete);
    }

    public Task<ConsumeResult> ConsumeProduct(string transactionId, Action<ConsumeResult> onComplete = null)
    {
        return ExecuteWebGLRequest<ConsumeResult>(id => GameInterfaceBridge.ConsumeIAPProduct(id, transactionId), onComplete);
    }
}

[Serializable]
public class IAPProduct
{
    public string sku;
    public string title;
    public string description;
    public string imageURI;
    public string displayPrice;
    public float priceValue;
    public string priceCurrencyCode;
    public string currencyImageURI;
}

[Serializable]
public class IAPProductList
{
    public IAPProduct[] products = new IAPProduct[0];
}

[Serializable]
public class PurchaseResult
{
    public PurchaseDetail detail;
}

[Serializable]
public class PurchaseDetail
{
    public string sku;
    public string purchase;
}

[Serializable]
public class ConsumeResult
{
    public string status;
    public string message;
}

public class IAPEvent
{
    public string type;
    public PurchaseDetail detail;
}