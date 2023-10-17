using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ThreeScreen_Participant : Client_Object {
    private const string OffsetFileName = "threeScreenOffset";
    private ParticipantOrder _participantOrder;
    private SpawnType _spawnType;
    private Interactable_Object _interactableObject;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override ParticipantOrder GetParticipantOrder() {
        return _participantOrder;
    }

    public override void SetSpawnType(SpawnType _spawnTypeIn) {
        _spawnType = _spawnTypeIn;
    }

    public override void AssignFollowTransform(Interactable_Object MyInteractableObjectIn, ulong targetClient) {
        if (IsServer)
        {
            _interactableObject = MyInteractableObjectIn;
            NetworkObject.TrySetParent(_interactableObject.NetworkObject, false);
          
            AssignInteractable_ClientRPC(_interactableObject.GetComponent<NetworkObject>(), targetClient);
        }
    }
    [ClientRpc]
    private void AssignInteractable_ClientRPC(NetworkObjectReference MyInteractable, ulong targetClient)
    {
        Debug.Log(
            $"MyInteractable{MyInteractable.NetworkObjectId} targetClient:{targetClient}, OwnerClientId:{OwnerClientId}");
        if (MyInteractable.TryGet(out var targetObject))
        {
            if (targetClient == OwnerClientId)
            {

                var conf = new ConfigFileLoading();
                conf.Init(OffsetFileName);
                if (conf.FileAvalible())
                {
                    conf.LoadLocalOffset(out var localPosition, out var localRotation);
                    transform.localPosition = localPosition;
                    transform.localRotation = localRotation;
                }
                _interactableObject = targetObject.transform.GetComponent<Interactable_Object>();
                
            }
        }
        else
        {
            Debug.LogError(
                "Did not manage to get my Car assigned interactions will not work. Maybe try calling this RPC later.");
        }
    }

    public override void De_AssignFollowTransform(ulong targetClient, NetworkObject netobj)
    {
        if (IsServer)
        {
            NetworkObject.TryRemoveParent(false);
            _interactableObject = null;
            De_AssignFollowTransformClientRPC(targetClient);
            DontDestroyOnLoad(gameObject);
        }
    }

    [ClientRpc]
    private void De_AssignFollowTransformClientRPC(ulong targetClient)
    {
        //ToDo: currently we just deassigned everything but NetworkInteractable object and _transform could turn into lists etc...
        _interactableObject = null;
     
        DontDestroyOnLoad(gameObject);
        Debug.Log("De_assign Interactable ClientRPC");
        
    }
    public override Transform GetMainCamera() {
        return transform;
        
    }

    public override void CalibrateClient(ClientRpcParams clientRpcParams) {
        if (_interactableObject != null) {
            transform.position = _interactableObject.GetCameraPositionObject().position;
            transform.rotation = _interactableObject.GetCameraPositionObject().rotation;
            var conf = new ConfigFileLoading();
            conf.Init(OffsetFileName);
            conf.StoreLocalOffset(transform.localPosition, transform.localRotation);
        }
        Debug.Log("Not Sure yet how to CalibrateClient for the threeScreen");
    }

    public override void StartQuestionair(QNDataStorageServer m_QNDataStorageServer) {
        Debug.Log("Not Sure yet how to StartQuestionair for the threeScreen");
    }
}
