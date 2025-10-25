中文测试
using System;
using System.Collections.Generic;
using System.Linq;

namespace UserManagement
{
    public class UserService 中文测试
    {
        private List<User> users;
        
        public UserService()
        {
            users = new List<User>();
        }
        
        public void AddUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
                
            users.Add(user);
        }
        
        public User FindUserById(int id)
        {
            return users.FirstOrDefault(u => u.Id == id);
        }
        
        public List<User> GetAllUsers()
        {
            return new List<User>(users);
        }
        
        public void RemoveUser(int id)
        {
            var user = FindUserById(id);
            if (user != null)
            {
                users.Remove(user);
            }
        }
        
        public int GetUserCount()
        {
            return users.Count;
        }
    }
    
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        
        public User(int id, string name, int age, string email)
        {
            Id = id;
            Name = name;
            Age = age;
            Email = email;
        }
    }
}
