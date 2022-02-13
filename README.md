Latest version of my XNA/MonoGame implementation of a Wolfenstein 3D style
raycasting engine.

# Todo
 * Allow for these options
 ** Fullscreen on / off
 ** Keyboard only / Keyboard & Mouse
 ** Mouse Sensitivity
 ** Music Vol. & SFX Vol.

 * Add missing sound effects
 ** Shotguns
 ** Blake Pistol
 ** Magic Hand
 
 * Replace Start Screen Image
 * Put a nice end screen with link to youtube

# Nice to have
 * Do something about large text on lower resolutions
 * A better weapon selection, esp. Rocket Launcher
 * More variety in the SFX department
 * Doom & Duke Soundtracks
 * Shadow Warrior & Blood References


# Version History
 * Original Code has been abandoned in 2013, written in C# & XNA 4.0
 * January 2022 repair on the old code began
 * Removed sub-projects, 2D Top Down related code and deprecated Code (eg. Hardware Mouse)
 * Updated dependencies: XNA -> MonoGame, xTile -> Tiled, fmod SFX -> MonoGame.SoundEffects
 * Lots of bugfixes before first compile (600+ errors)
 * Many workarounds for the new map format were needed
 * Had to recreate the gamestatemanager and some logic for the ECS
 * RayCaster itself was still working fine
 * Biggest Problems: GameStates, ECS, TileMap, Scaling to different Resolutions, Update & Init issues
