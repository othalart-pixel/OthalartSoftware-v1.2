using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models.Integrations
{
    public class OthalartUpstreamDraft
    {
        public string SourceSystem { get; set; } = "Othalart";
        public string TargetFamily { get; set; } = "Kitsu/Zou";
        public UpstreamProjectDraft Project { get; set; } = new UpstreamProjectDraft();
        public List<UpstreamEntityDraft> Entities { get; set; } = new List<UpstreamEntityDraft>();
        public List<UpstreamTaskDraft> Tasks { get; set; } = new List<UpstreamTaskDraft>();
        public List<UpstreamAssignmentDraft> Assignments { get; set; } = new List<UpstreamAssignmentDraft>();
        public List<OthalartUpstreamMappingWarning> Warnings { get; set; } =
            new List<OthalartUpstreamMappingWarning>();
    }

    public class UpstreamProjectDraft
    {
        public string LocalId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Client { get; set; } = "";
        public string Company { get; set; } = "";
        public string Description { get; set; } = "";
        public string Currency { get; set; } = "CLP";
        public string ProductName { get; set; } = "";
        public string RequestType { get; set; } = "";
    }

    public class UpstreamEntityDraft
    {
        public string LocalId { get; set; } = "";
        public string Name { get; set; } = "";
        public string EntityType { get; set; } = "Asset";
        public string ProductName { get; set; } = "";
        public string Category { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public double DurationPerUnit { get; set; } = 0.0;
        public string DurationUnit { get; set; } = "";
        public string QuantityUnit { get; set; } = "";
    }

    public class UpstreamTaskDraft
    {
        public string LocalId { get; set; } = "";
        public string EntityLocalId { get; set; } = "";
        public string Name { get; set; } = "";
        public string TaskType { get; set; } = "";
        public string Department { get; set; } = "";
        public string Status { get; set; } = "todo";
        public string DependsOnTaskLocalId { get; set; } = "";
        public string EquationKey { get; set; } = "";
        public string RequiredRole { get; set; } = "";
        public double EstimatedPersonDays { get; set; } = 0.0;
        public double EstimatedHours { get; set; } = 0.0;
        public double EstimatedCostCLP { get; set; } = 0.0;
    }

    public class UpstreamAssignmentDraft
    {
        public string LocalId { get; set; } = "";
        public string TaskLocalId { get; set; } = "";
        public string PersonLocalId { get; set; } = "";
        public string PersonName { get; set; } = "";
        public string RequiredRole { get; set; } = "";
        public double Hours { get; set; } = 0.0;
        public bool IsGenericResource { get; set; } = false;
    }

    public class OthalartUpstreamMappingWarning
    {
        public string Severity { get; set; } = "Warning";
        public string Scope { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
