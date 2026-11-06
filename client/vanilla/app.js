const form = document.getElementById('tripForm');
form.addEventListener('submit', async (event) => {
    event.preventDefault();
    const destination = document.getElementById('destination');
    const destinationValue = destination.value;
    const startDate = document.getElementById('startDate');
    const startDateValue = startDate.value;
    const endDate = document.getElementById('endDate');
    const endDateValue = endDate.value;
    const budget = document.getElementById('budget');
    const budgetValue = budget.value;
    const interests  = document.querySelectorAll('input[name="interests"]:checked');
    const interestsValues = Array.from(interests).map(interest => interest.value);
    const button = form.querySelector('button');
    button.disabled = true;
    button.textContent = 'Planning...';
    try{
        const response = await fetch("http://localhost:5086/api/trips/plan",{
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                destination: destinationValue,
                startDate: startDateValue,
                endDate: endDateValue,
                budget: budgetValue,
                interests: interestsValues
            })
        });
        const data = await response.json();
        //console.log(data);
        const results = document.getElementById('results');
        results.innerHTML = ``;
        data.days.forEach(day => {
            const card = document.createElement('div');
            card.className = 'day-card';
            const heading = document.createElement('h2');
            heading.textContent = `Day ${day.dayNumber} - ${day.date}`;
            card.appendChild(heading);
            const summary = document.createElement('p');
            summary.textContent = day.summary;
            card.appendChild(summary);
            
            day.activities.forEach(activity => {
                const row = document.createElement('div');
                row.className = 'activity';
                const time = activity.startTime ? activity.startTime.slice(0, 5) : '';
                row.textContent = `${time} - ${activity.name} (${activity.location??'No location provided'})`;
                card.appendChild(row);
            });
            results.appendChild(card);
        });
    }
    catch(error){
        console.error('Error:', error);
        document.getElementById('results').innerHTML = `<p class="error">An error occurred while planning the trip. Please try again later.</p>`;
    }
    finally{
        button.disabled = false;
        button.textContent = 'Plan Trip';
    }
});
