using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Newtonsoft.Json;
using UnityEngine;
using Timberborn.BeaversUI;
using Timberborn.BotsUI;
using Timberborn.Characters;
using Timberborn.GameFactionSystem;
using Timberborn.Localization;
using Timberborn.EntityPanelSystem;
using Timberborn.EntitySystem;
using Timberborn.Wellbeing;

namespace Mods.WebUI.Scripts
{
  internal class CharacterInformation
  {
    private readonly EntityRegistry _entityRegistry;
    private readonly EntityBadgeService _entityBadgeService;
    private readonly FactionService _factionService;
    private readonly ILoc _loc;
    private readonly TextureHandler _textureHandler;

    public CharacterInformation(WebUIServer webUIServer,
                                EntityRegistry entityRegistry,
                                EntityBadgeService entityBadgeService,
                                FactionService factionService,
                                ILoc loc,
                                TextureHandler textureHandler)
    {
      _entityRegistry = entityRegistry;
      _entityBadgeService = entityBadgeService;
      _factionService = factionService;
      _loc = loc;
      _textureHandler = textureHandler;

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

    string GetEntityAvatarPath(Character subject) {
      var _entityBadgesCache = new List<IEntityBadge>();
      subject.GetComponentsFast(_entityBadgesCache);
      foreach (var entityBadge in _entityBadgesCache) {
        if (entityBadge is BeaverEntityBadge) {
          return (entityBadge as BeaverEntityBadge).GetEntityAvatarPath();
        }
        if (entityBadge is BotEntityBadge) {
          return _factionService.Current.BotAvatar;
        }
      }
      return null;
    }

    public object GetJson()
    {
      string Url(string path) {
        if (path == null) {
          return null;
        }
        return _textureHandler.SignUrl($"/{path}.png?w=96");
      }
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
          Group = CharacterBatchControlTab.GetGroupingKey(o.EntityComponent),
        })
        .GroupBy(o => o.Group, o => new {
          Avatar = Url(GetEntityAvatarPath(o.Character)),
          Name = o.Character.FirstName,
          o.Character.Age,
          o.Character.GetComponentFast<WellbeingTracker>().Wellbeing,
        })
        .OrderBy(o => CharacterBatchControlTab.GetSortingKey(o.Key))
        .Select(g => new {
          Header = _loc.T(g.Key),
          Rows = g.OrderBy(o => o.Name),
        });
    }

  }
}
