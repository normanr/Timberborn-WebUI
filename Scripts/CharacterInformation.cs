using System.Linq;
using System.Web.Routing;
using Newtonsoft.Json;
using Timberborn.BaseComponentSystem;
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
        Icon = TextureUrl(building.GetComponent<LabeledEntitySpec>().Icon.Path),
        Tooltip = _loc.T(presentLocKey, _loc.T(building.GetComponent<LabeledEntitySpec>().DisplayNameLocKey)),
      };
    }

    IconTooltip Relation(Dweller d) {
      return BuildingData(d, d?.HasHome, d?.Home, "UI/Images/Game/homeless-nobg", "Beaver.Homeless", "Beaver.House");
    }

    IconTooltip Relation(Worker w) {
      return BuildingData(w, w?.Employed, w?.Workplace, "UI/Images/Game/jobless-nobg", "Beaver.Unemployed", "Beaver.Workplace");
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
          Character = entity.GetComponent<Character>(),
        })
        .Where(o => o.Character)
        .Select(o => new {
          o.EntityComponent,
          o.EntityComponent.GetComponent<SortableEntity>()?.SortableName,
          o.Character,
          Group = CharacterBatchControlTab.GetGroupingKey(o.EntityComponent),
        })
        .OrderBy(o => o.SortableName)
        .GroupBy(o => o.Group, o => new {
          Avatar = TextureUrl(_entityBadgeService.GetEntityAvatar(o.Character).AssetRefPath),
          Name = o.Character.FirstName,
          o.Character.Age,
          o.Character.GetComponent<WellbeingTracker>().Wellbeing,
          Home = Relation(o.EntityComponent.GetComponent<Dweller>()),
          Workplace = Relation(o.EntityComponent.GetComponent<Worker>()),
        })
        .OrderBy(o => CharacterBatchControlTab.GetSortingKey(o.Key))
        .Select(g => new {
          Header = _loc.T(g.Key),
          Rows = g,
        });
    }

  }
}
