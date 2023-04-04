Package : Super Retro World : ARPG Pack
This package contains many characters and monsters for your RPG game !

Author : Gif (Twitter : @gif_not_jif)

Notice :
The package contains the following folders :

___action_RPG_characters/.............Root folder for all the Character animation spritesheets if you want to make an Action-RPG
______character_[0-31]................Subfolder with (attack + shield animation) for each character
___action_RPG_setup_english...........Helpers to setup attack animation with characters files
___characters/........................Characters and monsters animated (walk animation)
    - srw_monsters_[0-5].png..........Monsters (each frame is 48x48, bottom side)

If you want to use the Action RPG attack animation spritesheets :
 - Beware that each frame is offset (the character is centered in the frame, instead of being stick to the bottom side). This is due to how RPG Maker
   anchor a character frame. You need to compensate that with a plugin/script (see below). The plugin/script will be used to move the character a few pixels back to its original place on his tile.

 - The plugin used (RM MV or MZ) is available here (credit to Shaz) : https://forums.rpgmakerweb.com/index.php?threads/character-anchors.105599/ (filename is :Shaz_CharacterAnchors.js)
	- add the plugin file (Shaz_CharacterAnchors.js) in the "js\plugins\" subfolder of your project
	- activate the plugin from the Plugin Manager window in RPG MV/MZ editor
		- use the script call : $gamePlayer.setAnchorY(0.65) to set correctly the ARPG attack animation
		- follow the "action_RPG_setup_english/setup_MV_MZ" tutorial folder to animate your character

 - The script used (RM VXace) is available here (credit to V.M of D.T) : https://forums.rpgmakerweb.com/index.php?threads/event-position-fine-tuning.1283/
	- copy the script content in the Script Editor window (in RPG Maker VXace editor), under the "Materials" section
	- use a "moveRoute" command in a event (target = Player) and enter the script call : offset(0,21)
		- this script call will offset the Player character 21 pixel to Y axis (south direction). This offset is needed to correctly align the atack animation to the current tile.
		- follow the "action_RPG_setup_english/setup_VXace" tutorial folder to animate your character

I hope this tutorials will helps you in your gamedev journey :)

I do not own those script/plugins, so I won't provide any support on them (I just use them to setup my assets). If you need help regarding the assets or my setup, you can contact me at pixelart.asset@gmail.com

Enjoy !
