using UnityEngine;
using BackEnd;
using BACKND.Database;

public class DatabaseManager : MonoBehaviour
{
    // 데이터베이스 클라이언트 인스턴스
    public static Client DBClient;

    public void InitializeDatabase()
    {
        // 1. 뒤끝 초기화 및 로그인 (선행 필수)
        Backend.InitializeAsync(bro =>
        {
            if (bro.IsSuccess())
            {
                Backend.BMember.CustomLogin("user1", "1234", async loginBro =>
                {
                    if (loginBro.IsSuccess())
                    {
                        // 2. 데이터베이스 클라이언트 생성
                        // 뒤끝 콘솔 > 데이터베이스 관리 메뉴에서 발급받은 DB UUID를 입력하세요.
                        DBClient = new Client("019c2d01-9928-7e2f-b049-1cdcdd6ac671");

                        // 3. 데이터베이스 초기화
                        await DBClient.Initialize();

                        Debug.Log("데이터베이스 초기화 완료");
                    }
                });
            }
        });
    }

    [Table("table_name")]
    public class MyTable : BaseModel
    {

    }
}
