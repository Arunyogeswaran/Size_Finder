using Microsoft.AspNetCore.Mvc;
using Size_Finder.Models;
using Size_Finder.Services;

namespace Size_Finder.Controllers
{
    public class SizeFinderController : Controller
    {
        private readonly PdfService _pdfService;
        private readonly ShopifyService _shopifyService;

        public SizeFinderController(PdfService pdfService, ShopifyService shopifyService)
        {
            _pdfService = pdfService;
            _shopifyService = shopifyService;
        }

        public IActionResult Index()
        {
            return View(new SizeFinderModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(SizeFinderModel model)
        {
            // ✅ Removed ModelState.IsValid check — process regardless
            var result = GetRecommendedSize(model);
            model.RecommendedSize = result.Size;
            model.FitNote = result.Note;

            if (!string.IsNullOrEmpty(model.ShopifyProductId))
            {
                var variant = await _shopifyService
                    .FindVariantBySize(model.ShopifyProductId, model.RecommendedSize);

                model.ShopifyVariantInfo = variant != null
                    ? $"✅ Size {variant.Option1} — ${variant.Price} " +
                      $"({variant.InventoryQuantity} in stock) | Variant ID: {variant.Id}"
                    : "⚠️ This size is currently unavailable for this product.";
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult ExportPdf(SizeFinderModel model)
        {
            var result = GetRecommendedSize(model);
            model.RecommendedSize = result.Size;
            model.FitNote = result.Note;

            var pdfBytes = _pdfService.GenerateSizeGuidePdf(model);
            return File(pdfBytes, "application/pdf", "MensSizeGuide.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> GetShopifyVariant(string productId, string size)
        {
            var variant = await _shopifyService.FindVariantBySize(productId, size);
            if (variant == null)
                return Json(new { success = false, message = "Size not found" });

            return Json(new
            {
                success = true,
                variantId = variant.Id,
                size = variant.Option1,
                price = variant.Price,
                stock = variant.InventoryQuantity
            });
        }

        private (string Size, string Note) GetRecommendedSize(SizeFinderModel model)
        {
            // ✅ Always convert to inches before calculating
            double chest = model.Unit == "cm" ? model.Chest / 2.54 : model.Chest;
            double waist = model.Unit == "cm" ? model.Waist / 2.54 : model.Waist;

            // ✅ Guard against zero values
            if (chest <= 0 || waist <= 0)
                return ("Unknown", "Please enter valid measurements.");

            string baseSize = chest switch
            {
                <= 34 => "XS",
                <= 36 => "S",
                <= 38 => "M",
                <= 40 => "L",
                <= 42 => "XL",
                <= 44 => "XXL",
                _ => "3XL"
            };

            string waistSize = waist switch
            {
                <= 28 => "XS",
                <= 30 => "S",
                <= 32 => "M",
                <= 34 => "L",
                <= 36 => "XL",
                <= 38 => "XXL",
                _ => "3XL"
            };

            var sizes = new List<string> { "XS", "S", "M", "L", "XL", "XXL", "3XL" };
            int finalIndex = Math.Max(sizes.IndexOf(baseSize), sizes.IndexOf(waistSize));

            if (model.FitPreference == "Loose" && finalIndex < sizes.Count - 1) finalIndex++;
            if (model.FitPreference == "Tight" && finalIndex > 0) finalIndex--;

            string note = model.BuildType switch
            {
                "Athletic" => "Athletic build — consider sizing up for better shoulder fit.",
                "Slim" => "Slim fit styles will complement your build perfectly.",
                "Heavy" => "We recommend going one size up for extra comfort.",
                _ => "Regular fit works great for your build type."
            };

            return (sizes[finalIndex], note);
        }
    }
}