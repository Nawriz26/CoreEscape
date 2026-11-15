/*
**
** File              : OwnerNetworkTransform.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 14, 2026
** Description       : Makes the owning client authoritative over
**                     its player's synchronized movement.
**
*/

using Unity.Netcode.Components;

public class OwnerNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}