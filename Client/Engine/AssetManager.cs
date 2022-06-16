using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

public class AssetManager
{
    private Dictionary<string, SpriteSheet> spriteSheets;
    protected ContentManager contentManager;

    public AssetManager(ContentManager content)
    {
        contentManager = content;
        spriteSheets = new Dictionary<string, SpriteSheet>();
    }

    public void LoadSpriteSheet(string assetname)
    {
        if (assetname == "")
        {
            throw new Exception("must specify an existing assetname");
        }
        SpriteSheet spriteSheet = new SpriteSheet(assetname);
        spriteSheets.Add(assetname, spriteSheet);
    }

    public Texture2D GetSprite(string assetname)
    {
        if (assetname == "")
        {
            throw new Exception("must specify an existing assetname");
        }
        return contentManager.Load<Texture2D>(assetname);
    }

    public SpriteSheet GetSpriteSheet(string assetname)
    {
        if (!spriteSheets.ContainsKey(assetname))
        {
            LoadSpriteSheet(assetname);
        }
        return spriteSheets.GetValueOrDefault(assetname);
    }

    public void PlaySound(string assetName)
    {
        SoundEffect snd = contentManager.Load<SoundEffect>(assetName);
        snd.Play();
    }

    public void PlayMusic(string assetName, bool repeat = true)
    {
        string songFileName = @"Content/" + assetName + ".ogg";
        var uri = new Uri(songFileName, UriKind.Relative);
        var song = Song.FromUri(assetName, uri);
        Microsoft.Xna.Framework.Media.MediaPlayer.IsRepeating = repeat;
        Microsoft.Xna.Framework.Media.MediaPlayer.Play(song);//.Load<Song>(assetName));
    }

    public ContentManager Content
    {
        get { return contentManager; }
    }
}