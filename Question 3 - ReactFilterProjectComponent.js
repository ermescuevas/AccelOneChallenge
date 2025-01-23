import React, { useState, useEffect } from 'react';

const ProjectFetchComponent = () => {
  const [projects, setProjects] = useState([]);
  const [filteredProjects, filterProjects] = useState([]);
  const [searchTerm, processSearch] = useState('');
  const [error, showError] = useState(null);
  const [loading, stateLoading] = useState(true);

  useEffect(() => {
    const getProjects = async () => {
      try {
        const response = await fetch('YOUR_API_URL_HERE');
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
        const data = await response.json();
        setProjects(data);
        filterProjects(data);
      } catch (err) {
        showError(err.message);
      } finally {
        stateLoading(false);
      }
    };

    getProjects();
  }, []);

  const onSearchChange = (e) => {
    const value = e.target.value;
    processSearch(value);

    const filtered = projects.filter((project) =>
      project.name.toLowerCase().includes(value.toLowerCase())
    );

    filterProjects(filtered);
  };

  if (loading) return <p>Loading projects...</p>;
  if (error) return <p>Error: {error}</p>;

  return (
    <div>
      <h1>Project List</h1>

      <input
        type="text"
        placeholder="Search projects by name"
        value={searchTerm}
        onChange={onSearchChange}
      />

      {filteredProjects.length === 0 ? (
        <p>No projects found.</p>
      ) : (
        <ul>
          {filteredProjects.map((project) => (
            <li key={project.id}>{project.name}</li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default ProjectFetchComponent;

/*
Explanation:


projects: Stores the list of projects fetched from the API.
filteredProjects: Get the filtered projects when the user perform a search.
searchTerm: Is the text that inputs the user from the search bar
loading and error: Handle loading and error states during the API fetch process.
The useEffect fetches data from an API when the component mounts. The fetched data is stored in projects and initially in filteredProjects.
The onSearchChange function updates the searchTerm and filters the projects array based on the search input. 
Validates if exists the project using the project name and toLower for be case insensitive

If the fetch data is loading, shows a loading message. But if there is an error displays a message
Once the data is available, it shows a list of projects or a message if no projects match the search term.
*/