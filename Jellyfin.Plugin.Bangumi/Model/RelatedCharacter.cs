#if EMBY
using PersonEntityType = MediaBrowser.Model.Entities.PersonType;
#else
using Jellyfin.Data.Enums;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.Bangumi.Model;

public class RelatedCharacter
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public CharacterType Type { get; set; }

    public Dictionary<string, string> Images { get; set; } = new();

    [JsonIgnore]
    public string? DefaultImage => Images?["large"];

    public string Relation { get; set; } = "";

    public Person[]? Actors { get; set; }

    public IEnumerable<PersonInfo> ToPersonInfos()
    {
        if (Actors == null)
            return Enumerable.Empty<PersonInfo>();
        return Actors.Select(actor =>
        {
            var info = new PersonInfo
            {
                Name = actor.Name,
                Role = Name,
                ImageUrl = actor.DefaultImage,
#if EMBY
                Type = PersonEntityType.Actor
#else
                Type = PersonKind.Actor
#endif
            };
            info.ProviderIds.Add(Constants.ProviderName, $"{actor.Id}");
            return info;
        });
    }
}