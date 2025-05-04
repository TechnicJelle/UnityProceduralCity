# Unity Procedural City

A procedural city generation setup in Unity, in the style of Paris from Assassin's Creed: Unity.

Source: https://github.com/TechnicJelle/UnityProceduralCity

## Features
- Super cool road generator, from which the rest of the city is generated
- Most intensive generation steps things happen on a separate thread, so ensure Unity doesn't freeze up during generation
- Simple depth fog effect
- Krita texture importer, so .kra files can be directly used as textures, just like any normal image file

## Potential Future Features
- Snapping the road to the sculpted terrain
- Way to designate areas with different generator values (Step Distance, for example), 
  so you can have a city with a dense center and less dense outskirts. And for the island.
- Put back the threading, by converting the Unity Colliders to my own Bounding Polygons beforehand,
  and then doing all checks in a thread again. Because those can safely run outside of Unity.

## Visuals and WIP screenshots

![image](https://github.com/user-attachments/assets/598d339e-7df1-4521-b260-b2507b085beb)

![image](https://github.com/user-attachments/assets/57e9b4b5-e5ed-401a-883c-89e080238c17)

![image](https://github.com/user-attachments/assets/0a09e1ca-f607-4393-8549-775eafe85140)

![image](https://github.com/user-attachments/assets/e84a129d-725f-4c5d-a308-179b6bfc3bc1)

![image](https://github.com/user-attachments/assets/bdd04671-7abd-4236-ba22-f5ee219f18b7)

![image](https://github.com/user-attachments/assets/a91a1549-f28a-44a7-8b56-dbba9220f4a2)

![image](https://github.com/user-attachments/assets/0b03d582-d2bb-4154-a146-25e1437a6e36)
