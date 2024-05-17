I'm New!

This is the code I used to create the KatanaScrap mod for Lethal Company.
I'm new to coding and modding, so you'll absolutely find ways I messed it up. I'm always open to suggestions or tips so feel free to let me know if you see some way to improve it.

Plugin.cs is used for registering the item through LethalLib and letting it spawn on moons, as well as generating a config to disable the mod or adjust the scrap value. 
  It accidentally got overwritted by Unity so I decompiled it and used that instead of rewriting the whole thing, that's why it looks a bit wonky. Always backup your projects with a different name.
SwordItem.cs is a modified script of the Knife from the base game, adjusted to not conflict with the knife's script as well as changing the range (although it is still a bit buggy, sorry)

Credit to BoboBoba, WhiteSpike, and Xu Xiaolan for help with the coding and item plugins.
