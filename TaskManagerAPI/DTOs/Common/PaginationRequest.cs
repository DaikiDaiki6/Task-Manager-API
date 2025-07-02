/// <summary>
/// Pagination request parameters for paginated API endpoints
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Page number to retrieve (starts from 1)
    /// </summary>
    /// <value>Default value is 1. Invalid values are automatically corrected to 1.</value>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    /// <value>Default value is 10. Must be between 1 and 100. Invalid values are automatically corrected.</value>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets a valid page number, ensuring it's at least 1
    /// </summary>
    /// <returns>Valid page number (minimum 1)</returns>
    public int GetValidPage() => Page < 1 ? 1 : Page;
    
    /// <summary>
    /// Gets a valid page size, ensuring it's between 1 and 100
    /// </summary>
    /// <returns>Valid page size (between 1 and 100)</returns>
    public int GetValidPageSize() => PageSize < 1 ? 10 : (PageSize > 100 ? 100 : PageSize);
}