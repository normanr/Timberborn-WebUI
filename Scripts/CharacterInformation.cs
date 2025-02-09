using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Newtonsoft.Json;
using UnityEngine;
using Timberborn.BaseComponentSystem;
using Timberborn.BeaversUI;
using Timberborn.BotsUI;
using Timberborn.Buildings;
using Timberborn.Characters;
using Timberborn.DwellingSystem;
using Timberborn.GameFactionSystem;
using Timberborn.Localization;
using Timberborn.EntityPanelSystem;
using Timberborn.EntitySystem;
using Timberborn.Wellbeing;
using Timberborn.WorkSystem;

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
        var responseString = JsonConvert.SerializeObject(
          GetJson(),
          new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        response.ContentType = "application/json";
        return responseString;
      } catch (Exception e) {
        Debug.LogError(DateTime.Now.ToString("HH:mm:ss ") + "Web UI: Error: " + e);
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

    string TextureUrl(string path) {
      if (path == null) {
        return null;
      }
      return _textureHandler.SignUrl($"/{path}.png?w=96");
    }

    class IconTooltip {
      public string Icon;
      public string Tooltip;
    }

    IconTooltip BuildingData(BaseComponent relation, bool? active, BaseComponent building, string missingIcon, string missingLocKey, string presentLocKey) {
      if (relation == null) {
        return null;
      }
      if (!active.GetValueOrDefault()) {
        return new IconTooltip {
          Icon = TextureUrl(missingIcon),
          Tooltip = _loc.T(missingLocKey),
        };
      }
      return new IconTooltip {
        Icon = TextureUrl(building.GetComponentFast<LabeledEntity>().GetLabeledEntitySpec().ImagePath),
        Tooltip = _loc.T(presentLocKey, _loc.T(building.GetComponentFast<LabeledEntitySpec>().DisplayNameLocKey)),
      };
    }

    IconTooltip Relation(Dweller d) {
      return BuildingData(d, d?.HasHome, d?.Home, "ui/images/game/homeless-nobg", "Beaver.Homeless", "Beaver.House");
    }

    IconTooltip Relation(Worker w) {
      return BuildingData(w, w?.Employed, w?.Workplace, "ui/images/game/jobless-nobg", "Beaver.Unemployed", "Beaver.Workplace");
    }

    public object GetJson()
    {
      // CharacterBatchControlRowFactory has:
      // ✔ CharacterBatchControlRowItemFactory for EntityAvatar from EntityBadgeService
      // ✔ BeaverBuildingsBatchControlRowItemFactory for Home and Workplace from BeaverBuildingsBatchControlRowItem
      // * DeteriorableBatchControlRowItemFactory for Bot.Durability -- ui/images/game/ico-durability
      // * AdulthoodBatchControlRowItemFactory for Beaver.Adulthood -- ui/images/game/ico-child-grow
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
          Dweller = o.EntityComponent.GetComponentFast<Dweller>(),
          Worker = o.EntityComponent.GetComponentFast<Worker>(),
        })
        .GroupBy(o => o.Group, o => new {
          Avatar = TextureUrl(GetEntityAvatarPath(o.Character)),
          Name = o.Character.FirstName,
          o.Character.Age,
          o.Character.GetComponentFast<WellbeingTracker>().Wellbeing,
          Home = Relation(o.Dweller),
          Workplace = Relation(o.Worker),
        })
        .OrderBy(o => CharacterBatchControlTab.GetSortingKey(o.Key))
        .Select(g => new {
          Header = _loc.T(g.Key),
          Rows = g.OrderBy(o => o.Name),
        });
    }

  }
}
