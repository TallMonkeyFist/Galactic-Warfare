using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : NetworkBehaviour
{

    #region Data Structures

    [System.Serializable]
    public struct ServerWeaponInput
    {
        public bool swapWeapons;
        public bool reload;
        public bool fire;
        public float serverTime;
        public Vector3 position;
        public Vector3 forward;
        public uint commandNumber;

        public ServerWeaponInput(bool _swap, bool _reload, bool _fire, float _time, Vector3 _pos, Vector3 _forward, uint _command)
        {
            swapWeapons = _swap;
            reload = _reload;
            fire = _fire;
            serverTime = _time;
            position = _pos;
            forward = _forward;
            commandNumber = _command;
        }
    }

    public struct WeaponControllerState
    {
        public int activeWeaponIndex;
        public uint commandNumber;

        public WeaponControllerState(int weapon, uint command)
        {
            activeWeaponIndex = weapon;
            commandNumber = command;
        }
    }

    public struct WeaponState
    {
        public int reserveAmmo;
        public int loadedAmmo;
    }

    #endregion

    private int activeWeaponIndex;
    private List<Weapon> weapons = new List<Weapon>();

    [SyncVar(hook = nameof(SyncServerWeaponState))]
    private WeaponControllerState serverWeaponState;
    private WeaponControllerState clientWeaponState;

    public Transform MuzzleSocket = null;

    private IEnumerator swapWeaponsCoroutine;
    private Weapon activeWeapon;
    private int swappingToIndex = -1;

    private void activateWeapon(int _weaponIndex)
    {
        //If swapping to the new weapon already, just return
        if(swappingToIndex != -1 && swappingToIndex == _weaponIndex)
        {
            return;
        }
        //If not swapping and current weapon is the new weapon is the active weapon, then return
        else if (swappingToIndex == -1 && _weaponIndex == activeWeaponIndex)
        {
            return;
        }
        //If swapping to a different weapon, stop swapping to different weapon
        else if(swappingToIndex != -1)
        {
            StopCoroutine(swapWeaponsCoroutine);
            swappingToIndex = -1;
        }
        swapWeaponsCoroutine = swapWeapons(_weaponIndex);
        StartCoroutine(swapWeaponsCoroutine);
    }

    private IEnumerator swapWeapons(int _weaponIndex)
    {
        swappingToIndex = _weaponIndex;
        yield return new WaitForSeconds(activeWeapon.WeaponStats.ReloadTime);

        activeWeaponIndex = _weaponIndex;
        activeWeapon = weapons[activeWeaponIndex];
        swappingToIndex = -1;
    }

    private bool checkIfCanFire()
    {
        Weapon currentWeapon = weapons[activeWeaponIndex];
        return currentWeapon.CanFire();
    }

    private void updateController(bool _swap, bool _reload, bool _fire)
    {
        if(_swap)
        {
            int swappedWeapon = (activeWeaponIndex + 1) % weapons.Count;
            activateWeapon(swappedWeapon);
        }
        else if(_reload)
        {
            activeWeapon.TryReload();
        }
        else if(_fire)
        {

        }
    }

    #region Client
    
    private uint clientProcessNumber = 0;

    [ClientCallback]
    private void Update()
    {
        if(!hasAuthority) { return; }

        pollInputs();
    }

    [Client]
    private void pollInputs()
    {
        bool swapWeapons = Input.GetKeyDown(KeyCode.Alpha1);
        bool reloadKey = Input.GetKeyDown(KeyCode.R);
        bool fireKey = Input.GetKey(KeyCode.Mouse0);
        
        if(fireKey || reloadKey)
        {
            CmdSendInputToServer(swapWeapons, reloadKey, fireKey);
        }
    }

    [Client]
    private void SyncServerWeaponState(WeaponControllerState oldState, WeaponControllerState newState)
    {
        //Prevents host from running this because they are a server and client
        if (isServer) { return; }

        if (hasAuthority)
        {
            if (clientWeaponState.commandNumber <= newState.commandNumber)
            {
                activateWeapon(newState.activeWeaponIndex);
                clientWeaponState = newState;
                return;
            }
        }

        if(activeWeaponIndex != newState.activeWeaponIndex)
        {
            activateWeapon(newState.activeWeaponIndex);
        }
    }

    #endregion

    #region Server

    private uint serverProcessNumber = 0;
    private Queue<ServerWeaponInput> ServerCommands = new Queue<ServerWeaponInput>();

    [ServerCallback]
    private void FixedUpdate()
    {
        
    }

    [Command]
    private void CmdSendInputToServer(bool swap, bool reload, bool fire)
    {
        ServerWeaponInput clientInput = new ServerWeaponInput
            (swap, reload, fire, Time.time, 
            weapons[activeWeaponIndex].GetProjectileSpawnPosition(), 
            weapons[activeWeaponIndex].GetProjectileDirection(),
            serverProcessNumber++);

        ServerCommands.Enqueue(clientInput);
    }

    #endregion
}
