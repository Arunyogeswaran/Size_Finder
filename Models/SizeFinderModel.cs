using System.ComponentModel.DataAnnotations;

namespace Size_Finder.Models
{
    public class SizeFinderModel
    {
        [Required]
        [Range(1, 500, ErrorMessage = "Please enter a valid chest measurement")]
        public double Chest { get; set; }

        [Required]
        [Range(1, 500, ErrorMessage = "Please enter a valid waist measurement")]
        public double Waist { get; set; }

        [Required]
        [Range(1, 500, ErrorMessage = "Please enter a valid hips measurement")]
        public double Hips { get; set; }

        [Required]
        [Range(1, 300, ErrorMessage = "Please enter a valid height")]
        public double Height { get; set; }

        public string Unit { get; set; } = "inches";
        public string BuildType { get; set; }
        public string FitPreference { get; set; }
        public string RecommendedSize { get; set; }
        public string FitNote { get; set; }
        public string ShopifyProductId { get; set; }
        public string ShopifyVariantInfo { get; set; }

        // ✅ Always convert based on unit
        public double ChestInInches => Unit == "cm" ? Chest / 2.54 : Chest;
        public double WaistInInches => Unit == "cm" ? Waist / 2.54 : Waist;
        public double HipsInInches => Unit == "cm" ? Hips / 2.54 : Hips;
    }

    public class ShopifyVariant
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Option1 { get; set; }
        public int InventoryQuantity { get; set; }
        public string Price { get; set; }
    }

    public class ShopifyProduct
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public List<ShopifyVariant> Variants { get; set; }
    }

    public class ShopifyProductResponse
    {
        public ShopifyProduct Product { get; set; }
    }
}