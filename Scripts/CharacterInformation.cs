using System;
using System.Linq;
using System.Web.Routing;
using Newtonsoft.Json;
using Timberborn.BeaverContaminationSystem;
using Timberborn.Characters;
using Timberborn.Localization;
using Timberborn.EntityPanelSystem;
using Timberborn.EntitySystem;
using Timberborn.Wellbeing;
using UnityEngine;

namespace Mods.WebUI.Scripts
{
  internal class CharacterInformation
  {
    private static readonly string AdultLocKey = "Beaver.Adult.PrefabName";
    private static readonly string ChildLocKey = "Beaver.Child.PrefabName";
    private static readonly string ContaminatedLocKey = "Beaver.Population.Contaminated";
    private static readonly string BotLocKey = "Bot.PrefabName";

    private readonly EntityRegistry _entityRegistry;
    private readonly EntityBadgeService _entityBadgeService;
    private readonly ILoc _loc;

    public CharacterInformation(WebUIServer webUIServer, EntityRegistry entityRegistry, EntityBadgeService entityBadgeService, ILoc loc)
    {
      _entityRegistry = entityRegistry;
      _entityBadgeService = entityBadgeService;
      _loc = loc;

      webUIServer.MapGet("/characters", HandleRequest);
    }

    [OnMainThread]
    string HandleRequest(RequestContext requestContext) {
      var httpContext = requestContext.HttpContext;
      var response = httpContext.Response;
      try {
        var responseString = JsonConvert.SerializeObject(GetJson());
        response.ContentType = "application/json";
        return responseString;
      } catch (Exception e) {
        Debug.LogError("Error: " + e);
        response.StatusCode = 500;
        response.ContentType = "text/plain; charset=utf-8";
        return "Error: " + e + "\n";
      }
    }

    private static string GetGroupingKey(EntityComponent entityComponent)
    {
      Contaminable componentFast = entityComponent.GetComponentFast<Contaminable>();
      if (componentFast != null && componentFast.IsContaminated)
      {
        return ContaminatedLocKey;
      }
      return entityComponent.GetComponentFast<SimpleLabeledPrefab>().PrefabNameLocKey;
    }

    private static string GetSortingKey(string locKey)
    {
      if (locKey == AdultLocKey)
      {
        return "1";
      }
      if (locKey == ChildLocKey)
      {
        return "2";
      }
      if (locKey == ContaminatedLocKey)
      {
        return "3";
      }
      if (locKey == BotLocKey)
      {
        return "4";
      }
      throw new ArgumentOutOfRangeException(nameof(locKey), locKey, null);
    }

    public object GetJson()
    {
      // CharacterBatchControlRowFactory has:
      // ✔ CharacterBatchControlRowItemFactory for EntityAvatar from EntityBadgeService
      // * BeaverBuildingsBatchControlRowItemFactory for Home and Workplace from BeaverBuildingsBatchControlRowItem
      // * DeteriorableBatchControlRowItemFactory for Bot.Durability
      // * AdulthoodBatchControlRowItemFactory for Beaver.Adulthood
      // * WellbeingBatchControlRowItemFactory for ✔Wellbeing *Bonuses
      // * StatusBatchControlRowItemFactory for Active Statuses
      return _entityRegistry.Entities
        .Select(entity => new {
          EntityComponent = entity,
          Character = entity.GetComponentFast<Character>(),
        })
        .Where(o => o.Character)
        .Select(o => new {
          o.EntityComponent,
          o.Character,
          Group = GetGroupingKey(o.EntityComponent),
        })
        .GroupBy(o => o.Group, o => new {
          Avatar = _entityBadgeService.GetEntityAvatar(o.Character).name,
          Name = o.Character.FirstName,
          o.Character.Age,
          o.Character.GetComponentFast<WellbeingTracker>().Wellbeing,
        })
        .OrderBy(o => GetSortingKey(o.Key))
        .Select(g => new {
          Header = _loc.T(g.Key),
          Rows = g.OrderBy(o => o.Name),
        });
    }

  }
}
