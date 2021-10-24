# Rosalind.Core
Rosalind는 C#으로 작성된 순수 객체지향 디스코드 봇입니다.

## 특징
* 순수한 객체지향으로 설계되어 관리와 개발이 간단합니다. (빠르다고는 안했습니다.)
* 엔티티 프레임워크 (SqlKata)를 이용해 빠르고 편하게 SQL 쿼리를 처리합니다.

## 요구사항
### 시스템
* .Net Core 3.1이 설치되어 있는 윈도우, 리눅스, 매킨토시 컴퓨터
### Nuget 패키지
* BooruSharp
* Discord.Net.Labs.Commands
* Discord.Net.Labs.Core
* Discord.Net.Labs.Rest
* Discord.Net.Labs.WebSocket
* FluentArgs
* log4net
* Microsoft.CodeAnalysis.CSharp.Scripting
* Microsoft.Extensions.DependencyInjection
* MySql.Data
* SqlKata
* SqlKata.Execution
* System.Data.SqlClient
* System.Runtime.Caching

## 콘솔 명령줄
* -h : 도움말을 표시합니다.
* -c <설정 파일 경로> : 프로그램이 사용할 설정 파일의 경로를 나타냅니다.

## 데이터베이스
```sql
CREATE DATABASE IF NOT EXISTS `STONKS_DB`;
```

테이블은 프로그램이 알아서 생성합니다.

## 테이블
### 길드 테이블
| Name   | Description                                        |
|--------|----------------------------------------------------|
| ID     | 프로그램 내에서 관리를 위해 사용되는 아이디입니다. |
| USERID | 유저의 디스코드 아이디입니다.                      |
| COIN   | 현재 유저가 가지고 있는 코인입니다.                |

## 할 일
* [ ] Lavalink.Net 적용하기
* [ ] [SlashCommand 애트리뷰트](https://github.com/Discord-Net-Labs/Discord.Net-Labs/pull/52)가 완성되면 적용하기

## 개발자
* Kate Lin (https://katelin.xyz)

## 참고한 프로젝트들
* https://github.com/csnewcs/botnewbot
* https://github.com/Discord-Net-Labs/Discord.Net-Labs/tree/release/2.x/samples/02_commands_framework

## 라이선스
```
Copyright (C) 2021 Kate Lin

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see https://www.gnu.org/licenses/
```
