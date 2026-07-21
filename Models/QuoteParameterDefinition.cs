using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class QuoteParameterDefinition
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = "";
        public string DataType { get; set; } = "decimal";
        public string Unit { get; set; } = "";
        public string DefaultValue { get; set; } = "";
        public double MinValue { get; set; } = 0.0;
        public double MaxValue { get; set; } = 1000000.0;
        public List<string> Options { get; set; } = new List<string>();
        public bool IsRequired { get; set; } = true;
        public bool AffectsTime { get; set; } = true;
        public bool AffectsCost { get; set; } = true;
        public bool AffectsPrice { get; set; } = true;
        public bool AffectsBreakdown { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string HelpText { get; set; } = "";
    }

    public class ProductQuoteConfiguration
    {
        public string CommercialUnit { get; set; } = "unidades";
        public decimal Quantity { get; set; } = 1m;
        public List<QuoteParameterDefinition> Parameters { get; set; } =
            new List<QuoteParameterDefinition>();
        public Dictionary<string, string> Values { get; set; } =
            new Dictionary<string, string>();
        public bool HasShotBreakdown { get; set; } = false;
        public List<ProductShotBreakdownItem> Shots { get; set; } =
            new List<ProductShotBreakdownItem>();
    }

    public class ProductShotBreakdownItem
    {
        public string Name { get; set; } = "";
        public double DurationSeconds { get; set; } = 0.0;
        public string Density { get; set; } = "Media";
        public string Complexity { get; set; } = "Media";
        public int Characters { get; set; } = 1;
    }
}
