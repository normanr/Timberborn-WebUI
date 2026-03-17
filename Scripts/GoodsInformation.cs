using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Routing;
using Newtonsoft.Json;
using UnityEngine;
using Timberborn.BlueprintSystem;
using Timberborn.GameFactionSystem;
using Timberborn.Goods;
using Timberborn.Localization;
using Timberborn.ResourceCountingSystem;
using Timberborn.SingletonSystem;

namespace Mods.WebUI.Scripts
{
  internal class GoodsInformation: ILoadableSingleton
  {
    private readonly GoodsGroupSpecService _goodsGroupSpecService;
    private readonly ISpecService _specService;
    private readonly FactionService _factionService;
    private readonly ResourceCountingService _resourceCountingService;
    private readonly ILoc _loc;
    private readonly TextureHandler _textureHandler;

    private Dictionary<string, GoodGroupSpec> _goodGroupSpecs;
    private List<GoodSpec> _goodSpecs;

    public GoodsInformation(WebUIServer webUIServer,
                            GoodsGroupSpecService goodsGroupSpecService,
                            ISpecService specService,
                            ResourceCountingService resourceCountingService,
                            FactionService factionService,
                            ILoc loc,
                            TextureHandler textureHandler)
    {
      _goodsGroupSpecService = goodsGroupSpecService;
      _specService = specService;
      _factionService = factionService;
      _resourceCountingService = resourceCountingService;
      _loc = loc;
      _textureHandler = textureHandler;

      webUIServer.MapGet("/goods", HandleRequest);
    }

    public void Load()
    {
      _goodGroupSpecs = _specService.GetSpecs<GoodGroupSpec>().ToDictionary(s => s.Id);
      _goodSpecs = [.. _specService.GetSpecs<GoodSpec>().OrderBy(g => (_goodGroupSpecs[g.GoodGroupId].Order, g.GoodOrder))];
    }

    async Task<string> HandleRequest(RequestContext requestContext) {
      await Awaitable.MainThreadAsync();
      var response = JsonConvert.SerializeObject(
        GetJson(),
        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
      requestContext.HttpContext.Response.ContentType = "application/json";
      return response;
    }

    string TextureUrl(string path) {
      if (path == null) {
        return null;
      }
      return _textureHandler.SignUrl($"/{path}.png?w=64");
    }

    public object GetJson()
    {
      return _goodSpecs
        .Select(o => new {
          Good = o,
          GoodGroup = _goodGroupSpecs[o.GoodGroupId],
          Stock = _resourceCountingService.GetGlobalResourceCount(o.Id),
        })
        .GroupBy(g => g.GoodGroup.Id, o => new {
          Name = o.Good.PluralDisplayName.Value,
          Icon = TextureUrl(o.Good.Icon.Path),
          o.Stock.AllStock,
          o.Stock.AvailableStock,
          o.Stock.FillRate,
        })
        .Select(g => new {
          Group = _goodGroupSpecs[g.Key],
          AllRows = g,
          Rows = g.Where(r => r.AllStock != 0),
        })
        .Select(g => new {
          Group = new {
            Name = g.Group.DisplayName.Value,
            Icon = TextureUrl(g.Group.Icon.Path),
            SingleResourceGroup = g.Group.SingleResourceGroup ? (bool?)true : null,
            AvailableStock = g.Rows.Sum(g => g.AvailableStock),
            FillRate = g.Group.SingleResourceGroup ? (float?)g.AllRows.Single().FillRate : null,
            Placeholder = !g.Group.SingleResourceGroup && g.Rows.Count() == 0 ? _loc.T("TopBar.Nothing") : null,
          },
          Rows = !g.Group.SingleResourceGroup ? g.Rows : null,
        });
    }

  }
}
