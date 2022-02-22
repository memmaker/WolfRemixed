Latest version of my XNA/MonoGame implementation of a Wolfenstein 3D style
raycasting engine.

# Todo
 * Add missing sound effects
 ** Shotguns
 ** Blake Pistol
 ** Magic Hand
 
 * Get Fullscreen working

 * Replace Start Screen Image
 * Put a nice end screen with link to youtube
 
 * Exploding Barrels

# Nice to have
 * Do something about large text on lower resolutions
 * A better weapon selection, esp. Rocket Launcher
 * More variety in the SFX department
 * Doom & Duke Soundtracks
 * Shadow Warrior & Blood References
 * Real Push Walls
 * Lighting Effects
 * Skyboxes for outside areas
 * Destroyable Walls & Items


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
