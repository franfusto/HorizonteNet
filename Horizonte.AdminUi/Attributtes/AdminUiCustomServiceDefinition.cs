namespace Horizonte.AdminUi.Attributtes;


/// <summary>
/// Atributo utilizado para marcar un campo estático que define un servicio personalizado en la interfaz de usuario de administración.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class AdminUiCustomServiceDefinition : Attribute
{
}