// Generated from IR
class Order {
  final int number;
  final String title;
  final String dates;

  const Order({
    required this.number,
    required this.title,
    required this.dates
  });

  Order copyWith({int? number, String? title, String? dates}) {
    return Order(
      number: number ?? this.number,
      title: title ?? this.title,
      dates: dates ?? this.dates
    );
  }
}
