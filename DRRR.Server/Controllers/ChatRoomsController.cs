﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DRRR.Server.Services;
using DRRR.Server.Dtos;
using DRRR.Server.Security;
using System.Security.Claims;

namespace DRRR.Server.Controllers
{
    /// <summary>
    /// 聊天室控制器
    /// </summary>
    [Route("api/rooms")]
    public class ChatRoomsController : Controller
    {
        private readonly ChatRoomService _chatRoomService;

        public ChatRoomsController(ChatRoomService chatRoomService)
            => _chatRoomService = chatRoomService;

        /// <summary>
        /// 获取房间列表
        /// </summary>
        /// <param name="keyword">关键词</param>
        /// <param name="page">页码</param>
        /// <returns>表示异步获取房间列表的任务，如果创建失败则返回错误信息</returns>
        [HttpGet]
        [JwtAuthorize(Roles.Guest, Roles.User, Roles.Admin)]
        public async Task<ChatRoomSearchResponseDto> GetRoomList(string keyword, int page)
        {
            string hashid = User.FindFirst("uid").Value;
            Roles role = (Roles)Convert.ToInt32(User.FindFirst(ClaimTypes.Role).Value);
            return await _chatRoomService.GetRoomList(keyword, page, HashidsHelper.Decode(hashid), role);
        }
        /// <summary>
        /// 验证房间名
        /// </summary>
        /// <param name="name">房间名</param>
        /// <returns>异步获取验证结果的任务</returns>
        [HttpGet("room-name-validation")]
        [JwtAuthorize(Roles.User, Roles.Admin)]
        public async Task<JsonResult> ValidateRoomNameAsync(string name) => Json(new
        {
            Error = await _chatRoomService.ValidateRoomNameAsync(name)
        });

        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="roomDto">用户输入的用于创建房间的信息</param>
        /// <returns>表示异步创建房间的任务，如果创建失败则返回错误信息</returns>
        [HttpPost]
        [JwtAuthorize(Roles.User, Roles.Admin)]
        public async Task<ChatRoomCreateResponseDto> CreateRoomAsync([FromBody]ChatRoomDto roomDto)
        {
            string hashid = User.FindFirst("uid").Value;
            return await _chatRoomService.CreateRoomAsync(HashidsHelper.Decode(hashid), roomDto);
        }

        /// <summary>
        /// 获取用户上一次登录时所在的房间ID
        /// </summary>
        /// <returns>表示获取用户上一次登录时所在的房间ID的任务</returns>
        [HttpGet("previous-room-id")]
        [JwtAuthorize(Roles.User, Roles.Admin)]
        public async Task<string> GetPreviousRoomIdAsync()
        {
            string hashid = User.FindFirst("uid").Value;
            return await _chatRoomService.GetPreviousRoomIdAsync(HashidsHelper.Decode(hashid));
        }

        /// <summary>
        /// 软删除连接信息
        /// </summary>
        /// <param name="roomHashid">房间哈希ID</param>
        /// <param name="userHashid">用户哈希ID</param>
        /// <returns>表示删除连接的任务</returns>
        [HttpPost("connection-soft-delete")]
        [JwtAuthorize(Roles.User, Roles.Admin, Roles.Guest)]
        public async Task<string> PerformSoftDeleteOfConnectionAsync([FromBody] ConnectionSoftDeleteRequestDto requestDto)
        {
            var userId = HashidsHelper.Decode(User.FindFirst("uid").Value);
            return await _chatRoomService
                .PerformSoftDeleteOfConnectionAsync(HashidsHelper.Decode(requestDto.RoomId), userId);
        }

        /// <summary>
        /// 申请进入房间
        /// </summary>
        /// <param name="id">房间哈希ID</param>
        /// <returns>表示申请进入房间的任务</returns>
        [HttpGet("entry-permission")]
        [JwtAuthorize(Roles.User, Roles.Admin, Roles.Guest)]
        public async Task<ChatRoomEntryPermissionResponseDto> ApplyForEntryAsync(string id)
        {
            int roomId = HashidsHelper.Decode(id);
            int userId = HashidsHelper.Decode(User.FindFirst("uid").Value);
            Roles userRole = (Roles)Convert.ToInt32(User.FindFirst(ClaimTypes.Role).Value);
            return await _chatRoomService.ApplyForEntryAsync(roomId, userId, userRole);
        }

        /// <summary>
        /// 验证房间密码
        /// </summary>
        /// <param name="entryRequestDto">房间ID和用户输入的密码</param>
        /// <returns>表示验证房间密码的任务</returns>
        [HttpPost("password-validation")]
        [JwtAuthorize(Roles.User, Roles.Admin, Roles.Guest)]
        public async Task<ChatRoomPasswordValidationResponseDto> ValidatePasswordAsync(
            [FromBody]ChatRoomPasswordValidationRequestDto entryRequestDto)
        {
            var roomId = HashidsHelper.Decode(entryRequestDto.RoomId);
            var userId = HashidsHelper.Decode(User.FindFirst("uid").Value);
            return await _chatRoomService
                .ValidatePasswordAsync(roomId, userId, entryRequestDto.Password);
        }

        /// <summary>
        /// 申请创建房间
        /// </summary>
        /// <returns>表示申请创建房间的任务</returns>
        [HttpGet("creating-permission")]
        [JwtAuthorize(Roles.User, Roles.Admin)]
        public async Task<JsonResult> ApplyForCreatingRoomAsync()
        {
            int userId = HashidsHelper.Decode(User.FindFirst("uid").Value);
            return Json(new
            {
                Error = await _chatRoomService.ApplyForCreatingRoomAsync(userId)
            });
        }
    }
}
