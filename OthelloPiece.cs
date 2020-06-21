using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OthelloPiece : MonoBehaviour
{
    [SerializeField] Material _material = null;

    Material _myMaterialA = null;
    Material _myMaterialB = null;
    [SerializeField] MeshRenderer _CylinderA = null;
    [SerializeField] MeshRenderer _CylinderB = null;
    private float _RotationRatio = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_RotationRatio > 0.0f)
        {
            transform.localEulerAngles = new Vector3(_RotationRatio * 180, 0.0f, 0.0f);
            transform.localPosition = new Vector3(transform.localPosition.x, 0.0f, transform.localPosition.z);
            _RotationRatio -= Time.deltaTime * 1.5f;
        }
    }

    public void SetColor(bool front)
    {
        if (_myMaterialA == null)
        {
            _myMaterialA = GameObject.Instantiate<Material>(_material);
            _myMaterialB = GameObject.Instantiate<Material>(_material);
            _CylinderA.material = _myMaterialA;
            _CylinderB.material = _myMaterialB;
        }
        _myMaterialA.color = front ? Color.white : Color.black;
        _myMaterialB.color = front ? Color.black : Color.white;
    }

    public void SetState(Othellosystem.ePieceState state)
    {
        bool isActive = (state != Othellosystem.ePieceState.None);
        {
            _CylinderA.gameObject.SetActive(isActive);
            _CylinderB.gameObject.SetActive(isActive);
        }
        SetColor(state == Othellosystem.ePieceState.Front);
    }

    public void StartTurnAnimation()
    {
        _RotationRatio = 1.0f;
    }
}