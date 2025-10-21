using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagement
{
    public class StudentManager
    {
        private List<Student> students;
        
        public StudentManager()
        {
            students = new List<Student>();
        }
        
        public void AddStudent(Student student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));
                
            students.Add(student);
        }
        
        public Student FindStudentById(int id)
        {
            return students.FirstOrDefault(s => s.Id == id);
        }
        
        public List<Student> GetAllStudents()
        {
            return new List<Student>(students);
        }
        
        public void RemoveStudent(int id)
        {
            var student = FindStudentById(id);
            if (student != null)
            {
                students.Remove(student);
            }
        }
        
        public int GetStudentCount()
        {
            return students.Count;
        }
    }
    
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        
        public Student(int id, string name, int age, string email)
        {
            Id = id;
            Name = name;
            Age = age;
            Email = email;
        }
    }
}
