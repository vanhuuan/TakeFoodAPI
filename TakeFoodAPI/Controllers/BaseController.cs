using Microsoft.AspNetCore.Mvc;

namespace TakeFoodAPI.Controllers;

/// <summary>
/// Base controller
/// </summary>
// [MapAuthorize(StatusAuthen.MustPermission, System.AttributeTargets.Class)]
public class BaseController : Controller
{
    /// <summary>
    /// Log 4 net
    /// </summary>
    // protected readonly log4net.ILog log = null;

    /// <summary>
    /// Base constructor
    /// </summary>
    protected BaseController()
    {
        // log = log4net.LogManager.GetLogger(GetType());
    }
}
