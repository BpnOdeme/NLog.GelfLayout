namespace NLog.Layouts.GelfLayout.Features.Masking
{
    public interface IMaskingService
    {
        /// <summary> Nesneyi maskeler (attribute + alan adı kuralları). Orijinal nesneyi bozmaz, yeni bir kopya (genellikle Dictionary veya string) döner. </summary>
        object? Mask(object? obj);

        /// <summary> JSON string'i alan adına göre maskeler. Bozuk JSON ise orijinali döndürür. </summary>
        string MaskJson(string json);

        /// <summary> Tekil string değeri verilen kuralla maskeler. </summary>
        string MaskValue(string? value, int prefix, int suffix, bool exclude);

        /// <summary> Objenin bilinen tipte olmadığı durumlar için (object/dictionary vb.) maskeler. </summary>
        object? MaskDynamic(object? value);
    }
}
