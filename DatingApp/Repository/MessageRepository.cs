﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Repository.Interfaces;
using DatingApp.Utils.Pagination;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void RemoveMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public IQueryable<MessageDto> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(m => m.DateSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.UserName && !u.RecipientDeleted),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.UserName && !u.SenderDeleted),
                _ => query.Where(u => u.RecipientUsername == messageParams.UserName && u.DateRead == null && !u.RecipientDeleted)
            };

            return query;
        }

        public async Task<IEnumerable<Message>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            return await _context.Messages
                .Include(m => m.Sender).ThenInclude(u => u.Photos)
                .Include(m => m.Recipient).ThenInclude(u => u.Photos)
                .Where(m => m.Recipient.UserName == currentUsername && !m.RecipientDeleted &&
                        m.Sender.UserName == recipientUsername ||
                        m.Recipient.UserName == recipientUsername && !m.SenderDeleted &&
                        m.Sender.UserName == currentUsername)
                .OrderBy(m => m.DateSent)
                .ToListAsync();
        }
    }
}
