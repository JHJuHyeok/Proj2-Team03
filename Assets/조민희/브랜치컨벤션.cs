/*

[브랜치 예시]
 - main        : 최종 제출용            

 - dev         : 개발 도중 사용하게 될 게임 파일  (보통의 경우 이곳에 병합)
 - test        : 개발된 기능 테스트
 - design/명칭 : 기능 변화 없는 시각적인 요소
 - feat/기능명 : 새로운 기능 개발
 - fix/기능명  : 발생한 버그 수정


### 브랜치 생성 규칙 ###

 1. main                                ☆☆☆절대 건들이지 말 것!!!☆☆☆
  ㄴ 제출용 최종 완성본 브랜치
  ㄴ 완성 판단 시 단 한 번만 병합

 2. dev
  ㄴ 개발 도중 병합용 브랜치
  ㄴ 각 브랜치 병합 시 오류 발생 여부 확인용
  ㄴ 또는 체크포인트 역할
  ㄴ dev 브랜치 병합 시 pull 필수                <- 이 때 팀장이 별도 공지 예정

 3. design
  ㄴ 기능이 아닌 시각적 요소 제작 시 사용
  ㄴ design/(만들 요소) 형식으로 생성할 것.
     ex) design/equipment
         design/shop
  ㄴ 가능하면 '씬 하나'에 하나의 브랜치를 사용할 것.
  ㄴ 씬과 분리된 요소(ex.팝업, HUD)의 경우, 별개의 브랜치 생성할 것

 4. feat
  ㄴ 한 기능에 하나의 브랜치 생성
     ex) feat/playerMove
         feat/enemySpawn
  ㄴ 너무 방대한 범위를 하나로 묶지 않을 것.
     ex) feat/battleScene    <- 안 됨
         feat/monster        <- 안 됨
  ㄴ 하위 브랜치로 정리 가능
     ex) feat/enemy/enemySpawn
         feat/shop/summonWeapon
  ㄴ 기능이 완성된 경우 브랜치 그대로 push, 병합 여부 확인 받을 것

 5. fix
  ㄴ 오류 혹은 버그 발생 시 생성
  ㄴ 오류가 생긴 기능의 이름을 그대로 사용할 것
     ex) feat/playerMove  ->  fix/playerMove



[커밋 예시]
 - 새로운 기능 추가    /   feat : 캐릭터 이동 기능 추가
 - 오류 수정           /   fix : 무기 데미지 적용 오류
 - 코드 리팩토링       /   refactor : 몬스터 스폰 기능 리팩토링
 - 기능 테스트         /   test : 맵 생성 기능 테스트
 - 환경,설정 수정      /   chore : 외부 에셋 추가
 - 파일,폴더명 수정    /   rename : resources -> Resources
 - 파일,폴더 삭제      /   remove : adressable 폴더 삭제
 - 주석 추가 or 변경   /   comment : 플레이어 컨트롤러 주석 추가


 ### 커밋을 각 과정마다 별개로 하는 것을 권장
  - 폴더 생성 시             chore : 'name' 폴더 생성
  - 새 기능 생성 시          feat : 'playerController' 스크립트 작성
  - 사전에 오류 발견 시      fix : 적과 충돌 미발생 오류 해결

  Q. 왜 이렇게 해야 하나요?
  A. 이러는 편이 어디서 오류가 생기는지 확인이 편합니다.
*/