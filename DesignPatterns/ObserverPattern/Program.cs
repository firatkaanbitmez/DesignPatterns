using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// Observer Pattern

class Program
{
    static async Task Main(string[] args)
    {
        // Ürünler
        var productA = new Product("Product A", 1000);
        var productB = new Product("Product B", 1200);

        // Gözlemciler
        var observer1 = new ConcreteObserver("Observer 1");
        var observer2 = new ConcreteObserver("Observer 2");

        // Ürünlere gözlemciler ekleniyor
        productA.Register(observer1);
        productB.Register(observer2);

        // Bildirimlerin yapıldığı iş parçacığı
        Task notificationTask = Task.Run(async () => await NotificationLoop());

        // Fiyatların değiştirilebildiği iş parçacığı
        Task priceChangeTask = Task.Run(async () => await PriceChangeLoop());

        // İki iş parçacığının tamamlanmasını bekle
        await Task.WhenAll(notificationTask, priceChangeTask);
    }

    // Fiyat değiştirme işlemi için kullanıcı girişi
    static async Task PriceChangeLoop()
    {
        while (true)
        {
            Console.WriteLine("\nEnter product name (A/B) to change price or type 'exit' to quit:");
            string productInput = Console.ReadLine();

            if (productInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            Console.WriteLine("Enter new price:");
            if (decimal.TryParse(Console.ReadLine(), out decimal newPrice))
            {
                if (productInput.Equals("A", StringComparison.OrdinalIgnoreCase))
                    await ProductManager.GetProductByName("Product A").ChangePriceAsync(newPrice);
                else if (productInput.Equals("B", StringComparison.OrdinalIgnoreCase))
                    await ProductManager.GetProductByName("Product B").ChangePriceAsync(newPrice);
                else
                    Console.WriteLine("Invalid product name.");
            }
            else
            {
                Console.WriteLine("Invalid price input.");
            }
        }
    }

    // Bildirimlerin yapıldığı döngü
    static async Task NotificationLoop()
    {
        while (true)
        {
            // Bu döngü içinde gözlemcilere bildirimler otomatik olarak yapılıyor
            await Task.Delay(1000); // Bildirimlerin sürekli olarak izlenmesi için küçük bir gecikme
        }
    }
}

// Gözlemci arayüzü (Observer)
public interface IObserver
{
    string ObserverName { get; set; }
    Task UpdateAsync(Product product);
}

// Ürün sınıfı (Observable)
public class Product
{
    private List<IObserver> _observers = new();
    public string Name { get; private set; }
    private decimal _price;

    public decimal Price
    {
        get => _price;
        private set
        {
            if (_price != value)
            {
                var oldPrice = _price;
                _price = value;
                NotifyObserversAsync(oldPrice).Wait(); // Değişiklik olduğunda gözlemcilere bildir
            }
        }
    }

    public Product(string name, decimal price)
    {
        Name = name;
        _price = price;
        ProductManager.AddProduct(this); // Ürünü global yönetime ekle
    }

    // Gözlemciyi ekle
    public void Register(IObserver observer)
    {
        _observers.Add(observer);
    }

    // Gözlemciyi çıkar
    public void Unregister(IObserver observer)
    {
        _observers.Remove(observer);
    }

    // Fiyat değişikliği
    public async Task ChangePriceAsync(decimal newPrice)
    {
        if (newPrice != Price)
        {
            Price = newPrice; // Fiyatı güncelle
        }
        else
        {
            Console.WriteLine($"No price change for {Name}. No notification sent.");
        }
    }

    // Tüm gözlemcilere bildir
    private async Task NotifyObserversAsync(decimal oldPrice)
    {
        foreach (var observer in _observers)
        {
            await observer.UpdateAsync(this);
        }
        Console.WriteLine($"Price changed from {oldPrice:C2} to {Price:C2} for {Name}. Notifications sent.");
    }
}

// Somut gözlemci sınıfı (Concrete Observer)
public class ConcreteObserver : IObserver
{
    public string ObserverName { get; set; }

    public ConcreteObserver(string observerName)
    {
        ObserverName = observerName;
    }

    public async Task UpdateAsync(Product product)
    {
        // Burada asenkron işlemler yapılabilir, örneğin veri tabanına kaydetme, dış API çağrıları vb.
        await Task.Run(() =>
        {
            Console.WriteLine($"{ObserverName}: The price of '{product.Name}' is now {product.Price:C2}.");
        });
    }
}

// Ürün yönetim sınıfı (Product Manager)
public static class ProductManager
{
    private static Dictionary<string, Product> _products = new Dictionary<string, Product>();

    public static void AddProduct(Product product)
    {
        if (!_products.ContainsKey(product.Name))
        {
            _products.Add(product.Name, product);
        }
    }

    public static Product GetProductByName(string productName)
    {
        if (_products.ContainsKey(productName))
        {
            return _products[productName];
        }
        else
        {
            throw new Exception("Product not found!");
        }
    }
}
