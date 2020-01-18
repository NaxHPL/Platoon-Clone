# Platoon-Clone
A clone of the Ni No Kuni casino game Platoon. Made in Unity 2019.1.8f1.

## How to play:
Both players get dealt ten cards and must arrange them into five units. There is no maximum size to a unit, however each unit must have at least one card. Once all units are formed, both players draw a card to see who goes first. The player with the highest value card will start by choosing one of their own units and one of their opponent's units. The two chosen units will fight and the unit with the highest total value will win the round. To win the game, you must win three rounds.

#### Card values:
+ Pawns are numbered 2 - 10 and are worth their face value.
+ Jacks and Queens are each worth 10.
+ Kings are worth 10. Any unit containing a King will win EXCEPT when facing a unit with a Bishop (ace).
+ Bishops (aces) are worth 1. Any unit containing a Bishop will lose EXCEPT when facing a unit with a King.
+ Wizards (jokers) aren't worth any points, however they cause both players to swap units.

#### Other rules:
+ If both units contain a King, the unit with the highest total value wins.
+ If both units contain a Bishop, the unit with the highest total value wins.
+ If both units contain a Wizard, the units are swapped twice.
+ A unit cannot be comprised solely of a Wizard.
+ A unit cannot have more than two special cards (King, Bishop, Wizard) or have two of the same special card.
+ A unit cannot contain both a King and a Bishop.
