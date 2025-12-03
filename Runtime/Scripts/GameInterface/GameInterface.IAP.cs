using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

public partial class GameInterface
{
    public event Action<IAPEvent> OnIAPEvent;
    /// <summary>
    /// Call GameInterface.Instance.GetProducts() ONCE at the start of your game (after the GameInterface.init method has been resolved) to retrieve the list of available items. This method returns a Promise resolving to an array of products.
    /// <para>Empty Array: If the array is empty, no IAP items are available. In this case:</para>
    /// <list type="bullet">
    /// <item>Do not display IAP options in the shop.</item>
    /// <item>If no other ways to obtain products exist, consider hiding the shop entirely.</item>
    /// </list>
    /// <para>During testing, the order of the items may change, and not all items may be available intentionally. This is done to ensure that the implementation handles the product list correctly.
    /// In addition, the currency may also change, which will affect the previously specified price in USD. Therefore, the current displayPrice must always be used.</para>
    /// </summary>
    /// <param name="onComplete">Callback invoked when the request succeeds</param>
    /// <param name="onError">Callback invoked when the request fails</param>
    /// <returns></returns>
    public async Task<IAPProduct[]> GetProducts(Action<IAPProduct[]> onComplete = null, Action<string> onError = null)
    {
        if (!HasFeature("iap"))
        {
            onError?.Invoke("IAP feature is not available");
            return new IAPProduct[0];
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        return await ExecuteWebGLRequest<IAPProduct[]>(id => GameInterfaceBridge.GetProducts(id), onComplete, onError);
#else
        try
        {
            int delay = tester != null ? tester.getProductsDelay : 0;
            await Task.Delay(delay);
            
            GameInterfaceData data = FetchFamobiJson();
            IAPProduct[] products = data.iap.products.ToArray();

            for (int i = products.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (products[i], products[j]) = (products[j], products[i]);
            }

            onComplete?.Invoke(products);
            return products;
        }
        catch (Exception ex)
        {
            string errorMessage = $"Failed to get IAP products: {ex.Message}";
            onError?.Invoke(errorMessage);
            throw;
        }
#endif
    }

    /// <summary>
    /// Purchase a product with the given SKU.
    /// </summary>
    /// <param name="sku">The product SKU to purchase</param>
    /// <param name="onComplete">Callback invoked when the purchase succeeds</param>
    /// <param name="onError">Callback invoked when the purchase fails</param>
    /// <returns></returns>
    public async Task<PurchaseResult> BuyProduct(string sku, Action<PurchaseResult> onComplete = null, Action<string> onError = null)
    {
        if (!HasFeature("iap"))
        {
            onError?.Invoke("IAP feature is not available");
            return null;
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        return await ExecuteWebGLRequest<PurchaseResult>(id => GameInterfaceBridge.BuyIAPProduct(id, sku), onComplete, onError);
#else
        try
        {
            int delay = tester != null ? tester.buyProductDelay : 0;
            await Task.Delay(delay);

            PurchaseResult result = new PurchaseResult { detail = new PurchaseDetail { sku = sku, purchase = "test_purchase" } };
         
            OnIAPEvent?.Invoke(new IAPEvent { type = "PURCHASE_SUCCESS_EVENT", detail = result.detail });
            onComplete?.Invoke(result);
            return result;
        }
        catch (Exception ex)
        {
            string errorMessage = $"Failed to buy product {sku}: {ex.Message}";
            onError?.Invoke(errorMessage);
            throw;
        }
#endif
    }

    /// <summary>
    /// Consume a purchased product with the given transaction ID.
    /// </summary>
    /// <param name="transactionId">The transaction ID of the purchase to consume</param>
    /// <param name="onComplete">Callback invoked when the consume succeeds</param>
    /// <param name="onError">Callback invoked when the consume fails</param>
    /// <returns></returns>
    public Task<ConsumeResult> ConsumeProduct(string transactionId, Action<ConsumeResult> onComplete = null, Action<string> onError = null)
    {
        if (!HasFeature("iap"))
        {
            onError?.Invoke("IAP feature is not available");
            return null;
        }
        return ExecuteWebGLRequest<ConsumeResult>(id => GameInterfaceBridge.ConsumeIAPProduct(id, transactionId), onComplete, onError);
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

[Serializable]
public class IAPEvent
{
    public string type;
    public PurchaseDetail detail;
}

[Serializable]
public class IAPData
{
    public List<IAPProduct> products;
}