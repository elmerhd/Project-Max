using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Automation;

namespace Max
{
    public class SpotifyEngine
    {
        private const string SpotifyAppId = "SpotifyAB.SpotifyMusic_zpdnekdrzrea0!Spotify";
        protected const string _clientId = "fd64e2aea7934ffa9199c7e96edc4cf1";
        protected const string _secretId = "f8bafcdf1f694c33b4708bb6e603dc58";
        private SpotifyClient Spotify;
        private IList<SimplePlaylist> Playlists;
        private readonly MaxEngine MaxEngine;
        private string PlaylistFile = "spotify-playlist.aiml";
        private string AvailableDeviceId = null;
        private List<Device> AvailableDevices;
        private Timer Timer { get; set; }

        public SpotifyEngine(MaxEngine maxEngine)
        {
            this.MaxEngine = maxEngine;
            FixPlaylistFile();
            OpenConnection();
            Timer = new Timer();
            Timer.Interval = 1800000;
            Timer.Elapsed += TokenTimer;
            Timer.Start();
            maxEngine.BrainEngine.Log($"Loading Engine: {nameof(SpotifyEngine)}");
        }

        private void TokenTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                Task.Run(async() => {  await OpenConnection();});
            }
            catch (Exception ex)
            {
                MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: {ex.Message}");
            }
        }

        public void OpenSpotify()
        {
            MaxEngine.AppEngine.OpenApp(SpotifyAppId);
        }
        public void FixPlaylistFile()
        {
            FileInfo playlistFile = new FileInfo($"{MaxEngine.BrainFolder}/{PlaylistFile}");
            if (playlistFile.Exists) {
                playlistFile.Delete();
            }

        }

        public async Task OpenConnection()
        {
            var config = SpotifyClientConfig.CreateDefault();
            var server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await server.Stop();
                var tokenResponse = await new OAuthClient(config).RequestToken(new AuthorizationCodeTokenRequest(
                    _clientId, _secretId, response.Code, server.BaseUri
                ));

                Spotify = new SpotifyClient(config.WithToken(tokenResponse.AccessToken));
                Playlists = await Spotify.PaginateAll(await Spotify.Playlists.CurrentUsers());
                InitSpotifyPlaylist();
                GetAvailableDeviceID();
            };

            await server.Start();

            var loginRequest = new LoginRequest(server.BaseUri, _clientId, LoginRequest.ResponseType.Code)
            {
                Scope = new[] { Scopes.AppRemoteControl, Scopes.PlaylistModifyPrivate, Scopes.PlaylistModifyPublic, Scopes.PlaylistReadCollaborative, Scopes.PlaylistReadPrivate, Scopes.Streaming, Scopes.UgcImageUpload, Scopes.UserFollowModify, Scopes.UserFollowRead, Scopes.UserLibraryModify, Scopes.UserLibraryRead, Scopes.UserModifyPlaybackState, Scopes.UserReadCurrentlyPlaying, Scopes.UserReadEmail, Scopes.UserReadPlaybackPosition, Scopes.UserReadPlaybackState, Scopes.UserReadPrivate, Scopes.UserReadRecentlyPlayed, Scopes.UserTopRead }
            };
            BrowserUtil.Open(loginRequest.ToUri());

        }

        public async void GetAvailableDeviceID()
        {
            DeviceResponse deviceResponse = await Spotify.Player.GetAvailableDevices();
            AvailableDevices = deviceResponse.Devices;
            foreach(Device dv in deviceResponse.Devices)
            {
                if (dv.IsActive && !dv.IsRestricted)
                {
                    AvailableDeviceId = dv.Id;
                    break;
                }
            }
        }

        
        public SpotifyClient GetSpotifyClient()
        {
            return Spotify;
        }


        public void InitSpotifyPlaylist()
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: Initializing Playlist");
            List<Category> spotifyPlaylistCategories = new List<Category>();
            foreach (SimplePlaylist spl in Playlists)
            {
                MusicResponse musicResponse = new MusicResponse(spl.Uri);
                spotifyPlaylistCategories.Add(new Category($"play {spl.Name} playlist".ToLower(), musicResponse));
            }
            MaxBrain maxBrain = new MaxBrain(spotifyPlaylistCategories.ToArray());
            var file = $"{MaxEngine.BrainFolder}/{PlaylistFile}";
            maxBrain.save(file);
            this.MaxEngine.BrainEngine.Bot.loadAIML(file);
        }

        public void Resume()
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: Resume");
            Spotify.Player.ResumePlayback();
        }

        public void Pause()
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: Pause");
            Spotify.Player.PausePlayback();
        }

        public void Next()
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: Next");
            Spotify.Player.SkipNext();
        }

        public void Previous()
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: Previous");
            Spotify.Player.SkipPrevious();
        }

        public void Shuffle(bool shuffle)
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: \tShuffle: {shuffle}");
            Spotify.Player.SetShuffle(new PlayerShuffleRequest(shuffle));
        }

        public void Volume(int volume)
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: \tVolume:{volume}");
            Spotify.Player.SetVolume(new PlayerVolumeRequest(volume));
        }

        public async void TransferPlayback(string devicetypeorname)
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: \tTransfer Playback: {devicetypeorname}");
            DeviceResponse deviceResponse = await Spotify.Player.GetAvailableDevices();
            AvailableDevices = deviceResponse.Devices;
            List<string> deviceIds = new List<string>();
            foreach (Device dv in deviceResponse.Devices)
            {
                if (dv.IsActive && !dv.IsRestricted && (dv.Name.ToLower().Contains(devicetypeorname) || dv.Type.ToLower().Contains(devicetypeorname)))
                {
                    AvailableDeviceId = dv.Id;
                    deviceIds.Add(dv.Id);
                    break;
                }
            }
            if (deviceIds.Count > 0)
            {
                TransferPlayback(deviceIds);
            }
        }

        public void TransferPlayback(List<string> deviceIds)
        {
            Spotify.Player.TransferPlayback(new PlayerTransferPlaybackRequest(deviceIds) { Play = true });
        }

        public void Play(string uri)
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: \tPlaying: {uri}");
            if (uri.Contains("spotify:track:"))
            {
                IList<string> uris = new List<string>() { uri };
                Spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest() { Uris = uris, DeviceId = (AvailableDeviceId ?? MaxEngine.MaxConfig.SpotifyDeviceId) });
            }
            else if (uri.Contains("spotify:playlist:"))
            {
                Spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest() { ContextUri = uri, DeviceId = (AvailableDeviceId ?? MaxEngine.MaxConfig.SpotifyDeviceId) });
            }
            else if (uri.Contains("spotify:artist:"))
            {
                Spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest() { ContextUri = uri, DeviceId = (AvailableDeviceId ?? MaxEngine.MaxConfig.SpotifyDeviceId) });
            }
            else if (uri.Contains("spotify:album:"))
            {
                Spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest() { ContextUri = uri, DeviceId = (AvailableDeviceId ?? MaxEngine.MaxConfig.SpotifyDeviceId) });
            }
        }

        public async void BrowseArtists(string artist)
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: \tSearching Artist: {artist}");
            var searchResponse = await Spotify.Search.Item(new SearchRequest(SearchRequest.Types.Artist, artist) { Limit = 3 });
            List<FullArtist> artists = searchResponse.Artists.Items;
            if (artists.Count > 0)
            {
                FullArtist fullArtist = artists[0];
                if (fullArtist.Uri != null)
                {
                    Play(fullArtist.Uri);
                }
            }
        }

        public async void BrowseTrack(string track, string artist)
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: \tSearching Track: {track} - {artist}");
            var searchResponse = await Spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, $"{track} - {artist}") { Limit = 3 });
            List<FullTrack> tracks = searchResponse.Tracks.Items;
            if (tracks.Count > 0)
            {
                FullTrack fullTrack = tracks[0];
                if (fullTrack.Uri != null)
                {
                    Play(fullTrack.Uri);
                }
            }
        }

        public async void BrowseAlbum(string album, string artist)
        {
            MaxEngine.BrainEngine.Log($"{nameof(SpotifyEngine)}: \tSearching Album: {album} - {artist}");
            var searchResponse = await Spotify.Search.Item(new SearchRequest(SearchRequest.Types.Album, $"{album} - {artist}") { Limit = 3 });
            List<SimpleAlbum> albums = searchResponse.Albums.Items;
            if (albums.Count > 0)
            {
                SimpleAlbum simpleAlbum = albums[0];
                if (simpleAlbum.Uri != null)
                {
                    Play(simpleAlbum.Uri);
                }
            }
        }
    }
}
