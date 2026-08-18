namespace TmsApi.Application.Dtos;

public class LinkDto
{
    public string Rel { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public string Href { get; set; } = string.Empty;
}