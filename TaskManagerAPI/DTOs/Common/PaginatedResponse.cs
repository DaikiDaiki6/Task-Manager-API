using System;

namespace TaskManagerAPI.DTOs.Common;

/// <summary>
/// Generic paginated response wrapper for API endpoints that return multiple items
/// </summary>
/// <typeparam name="T">The type of items in the paginated collection</typeparam>
public class PaginatedResponse<T>
{
     /// <summary>
    /// Collection of items for the current page
    /// </summary>
    /// <value>List of items of type T. Empty list if no items found.</value>
    public List<T> Data { get; set; } = new();
    
    /// <summary>
    /// Current page number (starts from 1)
    /// </summary>
    /// <value>The page number that was requested and returned</value>
    public int Page { get; set; }
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    /// <value>Maximum number of items that can be returned in a single page</value>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    /// <value>Complete count of items in the entire collection, not just current page</value>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Total number of pages available
    /// </summary>
    /// <value>Calculated as ceiling(TotalCount / PageSize)</value>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Indicates whether there are more pages after the current page
    /// </summary>
    /// <value>True if Page &lt; TotalPages, otherwise false</value>
    public bool HasNextPage { get; set; }
    
    /// <summary>
    /// Indicates whether there are pages before the current page
    /// </summary>
    /// <value>True if Page &gt; 1, otherwise false</value>
    public bool HasPreviousPage { get; set; }
}
