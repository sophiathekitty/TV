# TV (Animated LCD System for Space Engineers)

this is a script for space engineers that will display an idle slideshow with an ingame clock on a block named "TV" (developed using the new Inset Entertainment Corner block). but i'm using the text panel image size for the scene background image. so in theory should work on a text panel too.

## scene addresses

it expects that you have setup a bunch of text panels with ```DB:``` at the beginning of thier custom name. ```DB:ShowName.Main.0``` is the block that stores the show info in its custom data.

the scene collection can load scenes from the scene database using the ```ShowName.SceneName.0.CustomData``` format. it only needs ```ShowName.SceneName``` to load a random scene from the blocks with the DB:ShowName.SceneName in their name. (it will expect that all of the blocks have both a CustomData and Text scene.)

## show info example:

```
type:show,title:Show Name,name:ShowName║type:playlist═Intro,Body║type:segmant,name:Intro═Intro.0.CustomData,Intro.0.Text║type:segmant,name:Body═Jokes,Jokes,Jokes║type:segment,name:Body,stock:4═Hey, space engineers!:This is a stock Scene
```

### show info parts: (separated by ```║```)

show info: this is to identify that this is a show and what the title is.

```
type:show,title:Show Name,name:ShowName
```

the main playlist: this is a list of segments and scenes to play

```
type:playlist═Intro,Body
```

this segment will play two specific scenes using their full addresss

```
type:segmant,name:Intro═Intro.0.CustomData,Intro.0.Text
```

this segment will play three random scenes from the Jokes scene list.

```
type:segmant,name:Body═Jokes,Jokes,Jokes
```

this will play 4 scenes from the Stock scene list. the text following the ```═``` will be added to the stock scenes.
all of the stock scenes need to be the same length. it adds them to the SceneCollection when building the show and will pick a random set to overlay 4 lines at a time to the scene when loading a Stock scene.

```
type:segment,name:Body,stock:4═Hey, space engineers!:This is a stock Scene
```

## animated scenes

these need to include the Monospace font images made with whips image converter.

```
type:animation,length:800,animation:loop║type:background,width:178,height:107═

║type:sprite,width:31,height:26,animation:random,delay:5,x:294,y:312═

:


```

again these are split up by ```║```...

scene info: namely the length is used to determain how long to play the scene before trigger the SceneDone event. (this is 800 tv.Play() calls from the main function)

```
type:animation,length:800,animation:loop
```

background image: needs type background... and then after the ```═``` should be the monospace font image.

```
type:background,width:178,height:107═


```

animated sprite: needs type sprite, and animation can be set to loop or random. the delay sets how many frames to wait before switching the sprite image. after the ```═``` should be a list of monospace font images separated by a ```:```

```
type:sprite,width:31,height:26,animation:random,delay:5,x:294,y:312═

:


```
