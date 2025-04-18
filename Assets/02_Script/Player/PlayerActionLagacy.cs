using UnityEngine;
using System.Linq;
public class PlayerActionLagacy: MonoBehaviour
{
    // 에디터에서 값이 변경될 때마다 호출
    private void OnValidate()
    {
        // 변경된 값 확인 
        DebugController.Log("OnValidate called, detectionRange: " + detectionRange);
    }

    #region components
    Transform plyerTransform;
    Animator animator;
    #endregion


    [SerializeField]float moveSpeed = 4f; //이동 속도 
    Vector2 dir = new Vector2(0f,0f);

    #region AutoInteracting
    [Header("AutoInteracting")]
    [SerializeField] float detectionRange = 5f; // 탐색 반경
    [SerializeField] float InteractionRange = 1f; // 상호작용 반경
    [SerializeField] Transform autoInteractTargetTransform; // 자동 상호작용 대상
    [SerializeField] bool isAutoInteracting = false; // 자동 상호작용 중인지(디버그용)
    #endregion

    [SerializeField] Collider2D[] colliders;
    private void Start()
    {
        plyerTransform = GetComponent<Transform>();
        animator = GetComponent<Animator>();

        // ㅇㅎㅈ 추가
        // 인벤토리에 플레이어 트랜스폼 캐싱
        InventoryManager.Instance.CachingPlayerTransform(transform);
    }

    void Update()
    {
        HandleMovement(); // 상하좌우 입력 관리 
        HandleMoveAnimation(); // 상하좌우로 움직일떄 애니메이션 관리 
        AutoInteract(); // 스페이스 바를 누르면 근처 오브젝트와 자동 상호작용


        // ㅇㅎㅈ 추가
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            CraftManager.Instance.ToggleCraftingUI();
        }
    }

    void HandleMovement()
    {
        dir.x = Input.GetAxisRaw("Horizontal") ;
        dir.y = Input.GetAxisRaw("Vertical");
        //DebugController.Log(dir.x + "," + dir.y);

        plyerTransform.position += new Vector3(dir.x, dir.y, 0).normalized* moveSpeed * Time.deltaTime;
    }

    void HandleMoveAnimation()
    {
        if (dir.x == 0 && dir.y == 0)
        {
            animator.SetBool("IsMove", false);
        }
        else
        {
            animator.SetBool("IsMove", true);
            // 이동했던 방향 기억 
            animator.SetFloat("PreDx", dir.x);
            animator.SetFloat("PreDy", dir.y);
        }

        animator.SetFloat("Dx", dir.x);
        animator.SetFloat("Dy", dir.y);
        
    }

    void AutoInteract()
    {
        //스페이스 바를 처음 누르게되면 주변에서 상호작용 가능한 가장 가까운 오브젝트를 찾음 
        if (Input.GetKeyDown(KeyCode.Space)) FindClosestInteractableObj();

        //스페이스바를 누르는 동안 타겟으로 이동 
        if (Input.GetKey(KeyCode.Space) && autoInteractTargetTransform != null) MoveTowardsTargetObj();

        //스페이스 바에서 손을 떼면 상호작용 중지 
        if (Input.GetKeyUp(KeyCode.Space)) StopAutoInteraction();

    }


    /// <summary>
    /// 상호작용 가능한 가장 가까운 오브젝트 탐색 
    /// </summary>
    void FindClosestInteractableObj()
    {
        DebugController.Log("FindClosestInteractableObj 실행 ");

        // 탐색 반경 내에 있는 Interactable 물체 탐지
        colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, LayerMask.GetMask("Interactable"));

        if (colliders.Length > 0)
        {
            autoInteractTargetTransform = colliders // Collider2D 배열을 
                .Select(collider => collider.transform) //transform 배열로 바꿔주고 (using System.Linq; 필요)
                .OrderBy(t => Vector2.Distance(transform.position, t.position)) //Distance 기준 오름차순으로 정렬 
                .FirstOrDefault(); // 첫번째 혹은 null 반환 ->  탐지된 물체가 없을 때도 오류 없이 처리가능
        }
        else
        {
            DebugController.Log("상호작용 가능한 오브젝트가 없습니다 ");
            autoInteractTargetTransform = null;
        }
    }

    /// <summary>
    /// 가장 가까운 상호작용 가능한 오브젝트로 이동 
    /// </summary>
    void MoveTowardsTargetObj()
    {
        if (autoInteractTargetTransform != null)
        {
            isAutoInteracting = true;

            Vector3 direction = (autoInteractTargetTransform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            //애니메이션 적용
            animator.SetBool("IsMove", true);
            animator.SetFloat("Dx", direction.x);
            animator.SetFloat("Dy", direction.y);
        }

        // 타겟 근처에 도달하면 상호작용
        if (Vector2.Distance(transform.position, autoInteractTargetTransform.position) < InteractionRange)
        {
            InteractWithTarget();
        }
    }

    /// <summary>
    /// 오브젝트 종류에 따라 알맞은 상호작용 실행 
    /// </summary>
    void InteractWithTarget()
    {
        DebugController.Log($"InteractWithTarget with : {autoInteractTargetTransform.tag}");

        switch (autoInteractTargetTransform.tag)
        {
            case "Item":
                GetItem(); //아이템습득 
                break;
            case "Monster":
                //공격
                break;
            case "Minerals":
                //채굴(광질)
                break;
            case "Plants":
                //채집  
                break;
            case "Trees":
                //도끼질 
                break;
            default:
                break;
        }
        


        // TODO : 지금은 아이템밖에 없어서 한번만 상호작용하면 되지만 나중에 나무캐기나 공격같은거 하면 여러번 해야하니까 이후 수정 필요 
        
        autoInteractTargetTransform = null; //상호작용 완료한 타겟은 없애고
        FindClosestInteractableObj();//새로운 타겟 탐색
        if (autoInteractTargetTransform == null) StopAutoInteraction(); // 새로운 타겟이 없으면 자동상호작용 초기화 

    }

    void StopAutoInteraction()
    {
        DebugController.Log("StopAutoInteraction");
        isAutoInteracting = false;
        animator.SetBool("IsMove", false);
        autoInteractTargetTransform = null;
    }

    void GetItem()
    {
        //DebugController.Log("GetItem 함수 실행됨");
        //autoInteractTargetTransform.gameObject.SetActive(false); //TODO : 일단 비활성화 되도록 했고 이후에 인벤토리에 들어가고 기타등등 수정 


        // ㅇㅎㅈ 추가
        Item gotItem = autoInteractTargetTransform.GetComponent<Item>();

        if (gotItem != null)
        {
            // 필드의 아이템을 인벤토리에 추가했다면
            if(InventoryManager.Instance.AddItem(gotItem.ItemData))
            {
                // 필드의 아이템은 지움
                Destroy(autoInteractTargetTransform.gameObject);
            }
        }
    }

    // 탐색 반경 디버그용 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, InteractionRange);
    }
}
