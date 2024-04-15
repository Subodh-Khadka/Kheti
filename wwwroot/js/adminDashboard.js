
var revenueDates = @Html.Raw(Json.Serialize(Model.RevenueDates));
var formattedDates = revenueDates.map(function (date) {
    return new Date(date).toLocaleDateString();
});
var revenueAmounts = @Html.Raw(Json.Serialize(Model.RevenueAmounts));

var ctx = document.getElementById('revenueChart').getContext('2d');

var revenueChart = new Chart(ctx, {
    type: 'line',
    data: {
        labels: formattedDates,
        datasets: [{
            label: 'Total Revenue',
            data: revenueAmounts,
            backgroundColor: 'rgba(54, 162, 235, 0.2)',
            borderColor: 'rgba(54, 162, 235, 1)',
            borderWidth: 1
        }]
    },
    options: {
        scales: {
            y: {
                beginAtZero: true
            }
        }
    }
});


var ctx = document.getElementById('userCategoryChart').getContext('2d');
var userCategoryChart = new Chart(ctx, {
    type: 'pie',
    data: {
        labels: ['Customers', 'Sellers', 'Experts'],
        datasets: [{
            data: [@Model.TotalCustomers, @Model.TotalSellers, @Model.TotalExperts],
            backgroundColor: [
                'rgba(255, 99, 132, 0.7)',
                'rgba(54, 162, 235, 0.7)',
                'rgba(75, 192, 192, 0.7)'
            ]
        }]
    },
});